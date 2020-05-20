using Agebull.Common;
using Agebull.Common.Logging;
using Consul;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class ConsulEventPoster : MessagePostBase, IMessagePoster, IFlowMiddleware
    {
        #region 消息可靠性

        private class ConsulQueueItem
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public byte[] Message { get; set; }
            public string FileName { get; set; }
            public int Try { get; set; }
        }
        private static readonly ConcurrentQueue<ConsulQueueItem> redisQueues = new ConcurrentQueue<ConsulQueueItem>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private CancellationTokenSource tokenSource;

        private async Task AsyncPostQueue()
        {
            await Task.Yield();
            //还原发送异常文件
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "consul");
            var isFailed = ReQueueErrorMessage(path);

            Logger.Information("异步消息投递已启动");
            while (ZeroFlowControl.IsAlive)
            {
                if (isFailed)
                {
                    await Task.Delay(1000);
                    isFailed = true;
                }
                if (redisQueues.Count == 0)
                {
                    try
                    {
                        await semaphore.WaitAsync(60000, tokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.Information("收到系统退出消息,正在退出...");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(() => $"错误信号.{ex.Message}");
                        isFailed = true;
                        continue;
                    }
                }

                while (redisQueues.TryDequeue(out ConsulQueueItem item))
                {
                    if (!await DoPost(Logger, path, item))
                    {
                        isFailed = true;
                        break;
                    }
                }
            }
            Logger.Information("异步消息投递已关闭");
        }
        /// <summary>
        /// 还原发送异常文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool ReQueueErrorMessage(string path)
        {
            var files = IOHelper.GetAllFiles(path, "*.msg");
            if (files.Count <= 0)
            {
                return false;
            }
            Logger.Information(() => $"载入发送提示消息,总数{files.Count}");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    redisQueues.Enqueue(SmartSerializer.ToObject<ConsulQueueItem>(json));
                }
                catch (Exception ex)
                {
                    Logger.Warning(() => $"{file} : 消息载入错误.{ex.Message}");
                }
            }

            return true;
        }

        private async Task<bool> DoPost(ILogger logger, string path, ConsulQueueItem item)
        {
            var state = false;
            try
            {
                var result = await client.Event.Fire(new UserEvent
                {
                    ID = item.ID,
                    Name = item.Name,
                    Payload = item.Message
                });
                state = result.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                logger.Warning(() => $"[异步消息投递] {item.ID} 发送失败.{ex.Message}");
                redisQueues.Enqueue(item);
                state = false;
            }
            if (state)
            {
                if (item.FileName != null)
                {
                    try
                    {
                        File.Delete(item.FileName);
                        logger.Warning(() => $"[异步消息投递] {item.ID} 发送成功,删除备份文件,{item.FileName}");
                    }
                    catch
                    {
                        logger.Warning(() => $"[异步消息投递] {item.ID} 发送成功,删除备份文件失败,{item.FileName}");
                    }
                }
                else
                {
                    logger.Debug(() => $"[异步消息投递] {item.ID} 发送成功");
                }
                return true;
            }
            //写入异常文件
            ++item.Try;
            if (item.FileName != null)
            {
                return false;
            }

            item.FileName = Path.Combine(path, $"{item.ID}.msg");
            logger.Warning(() => $"[异步消息投递] {item.ID} 发送失败,记录异常备份文件,{item.FileName}");
            try
            {
                File.WriteAllText(item.FileName, SmartSerializer.ToString(item));
            }
            catch (Exception ex)
            {
                item.FileName = null;
                logger.Error(() => $"[异步消息投递] {item.ID} 发送失败,记录异常备份文件失败.错误:{ex.Message}.文件名:{item.FileName}");
            }
            return false;
        }

        #endregion

        #region IMessagePoster

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            message.Offline();
            var item = new ConsulQueueItem
            {
                ID = message.ID,
                Name = message.Topic,
            };
            message.ID = null;
            message.Topic = null;
            message.Trace = null;
            item.Message = SmartSerializer.SerializeMessage(message).ToUtf8Bytes();
            redisQueues.Enqueue(item);
            semaphore.Release();
            message.RealState = MessageState.AsyncQueue;
            LogRecorder.MonitorDetails("[ConsulPoster.Post] 消息已投入发送队列,将在后台静默发送直到成功");
            return Task.FromResult<IMessageResult>(null);//直接使用状态
        }

        #endregion

        #region IFlowMiddleware

        /// <summary>
        /// 单例
        /// </summary>
        public static ConsulEventPoster Instance = new ConsulEventPoster();

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(ConsulEventPoster);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        private ConsulClient client;

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Open()
        {
            Logger.Information("ConsulPoster >>> Start");
            var consulUrl = $"http://{ConsulOption.Instance.ConsulIP}:{ConsulOption.Instance.ConsulPort}";
            client = new ConsulClient(x =>
            {
                x.Address = new Uri(consulUrl);
            });
            State = StationStateType.Run;
            tokenSource = new CancellationTokenSource();
            return AsyncPostQueue();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Close()
        {
            Logger.Information("ConsulPoster >>> Close");
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = null;
            State = StationStateType.Closed;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        Task ILifeFlow.Destory()
        {
            Logger.Information("ConsulPoster >>> Check");
            client.Dispose();
            return Task.CompletedTask;
        }

        #endregion

    }
}
