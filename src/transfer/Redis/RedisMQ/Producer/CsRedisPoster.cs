using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using CSRedis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis生产者
    /// </summary>
    public class CsRedisPoster : NewtonJsonSerializeProxy, IMessagePoster, IFlowMiddleware
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static CsRedisPoster Instance = new CsRedisPoster();


        #region 消息可靠性

        private class RedisQueueItem
        {
            public string ID { get; set; }
            public string Channel { get; set; }
            public string Message { get; set; }
            public int Step { get; set; }
            public string FileName { get; set; }
            public int Try { get; set; }
        }
        ILogger logger;
        private static readonly ConcurrentQueue<RedisQueueItem> redisQueues = new ConcurrentQueue<RedisQueueItem>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private CancellationTokenSource tokenSource;

        private async Task AsyncPostQueue()
        {
            try
            {
                client.Ping();
            }
            catch (RedisClientException ex)
            {
                logger.Error(ex.Message);
            }
            await Task.Yield();
            //还原发送异常文件
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "redis");
            var isFailed = ReQueueErrorMessage(path);

            logger.Information("异步消息投递已启动");
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
                        logger.Information("收到系统退出消息,正在退出...");
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.Warning(() => $"错误信号.{ex.Message}");
                        isFailed = true;
                        continue;
                    }
                }

                while (redisQueues.TryPeek(out RedisQueueItem item))
                {
                    if (!await DoPost(logger, path, item))
                    {
                        isFailed = true;
                        break;
                    }
                }
            }
            logger.Information("异步消息投递已关闭");
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
            logger.Information(() => $"载入发送错误消息,总数{files.Count}");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    redisQueues.Enqueue(JsonHelper.DeserializeObject<RedisQueueItem>(json));
                }
                catch (Exception ex)
                {
                    logger.Warning(() => $"{file} : 消息载入错误.{ex.Message}");
                }
            }

            return true;
        }

        private async Task<bool> DoPost(ILogger logger, string path, RedisQueueItem item)
        {
            var state = false;
            try
            {
                if (item.Step == 0)
                {
                    await client.SetAsync($"msg:{item.Channel}:{item.ID}", item.Message);
                    item.Step = 1;
                }
                if (item.Step == 1)
                {
                    await client.LPushAsync($"msg:{item.Channel}", item.ID);
                    item.Step = 2;
                }
                if (item.Step == 2)
                {
                    await client.PublishAsync(item.Channel, item.ID);
                    item.Step = 3;
                }
                redisQueues.TryDequeue(out _);
                state = true;
            }
            catch (Exception ex)
            {
                logger.Warning(() => $"[异步消息投递] {item.ID} 发送失败.{ex.Message}");
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
                File.WriteAllText(item.FileName, JsonHelper.SerializeObject(item));
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
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public Task<IMessageResult> Post(IInlineMessage message)
        {
            var item = new RedisQueueItem
            {
                ID = message.ID,
                Channel = message.Topic,
                Message = ToString(message, false)
            };
            LogRecorder.MonitorTrace("[CsRedisPoster.Post] 消息已投入发送队列,将在后台静默发送直到成功");
            redisQueues.Enqueue(item);
            semaphore.Release();
            message.State = MessageState.Accept;
            message.RuntimeStatus = ApiResultHelper.Helper.Waiting;
            return Task.FromResult(message.ToMessageResult());
        }

        #endregion

        #region IFlowMiddleware

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroMiddleware.Name => "RedisProducer";

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => 0;

        private CSRedisClient client;
        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(CsRedisPoster));
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Start()
        {

            client = new CSRedisClient(RedisOption.Instance.ConnectionString);
            State = StationStateType.Run;
            tokenSource = new CancellationTokenSource();
            _ = AsyncPostQueue();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            tokenSource?.Cancel();
            tokenSource.Dispose();
            tokenSource = null;
            State = StationStateType.Closed;
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        public void End()
        {
            client.Dispose();
        }

        #endregion
    }
}
/*
        
        private string DoProducer(string channel, string title, string content)
        {
            var item = new RedisQueueItem
            {
                ID = Guid.NewGuid().ToString("N"),
                Channel = channel,
                Message = JsonHelper.SerializeObject(new MessageItem
                {
                    Title = title,
                    Content = content
                })
            };
            redisQueues.Enqueue(item);
            semaphore.Release();
            return item.ID;
        }


        /// <inheritdoc/>
        public string Producer(string channel, string title, string content)
        {
            return DoProducer(channel, title, content);
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, JsonHelper.SerializeObject(content));
            return default;
        }

        /// <inheritdoc/>
        public void Producer<TArg>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, JsonHelper.SerializeObject(content));
        }
        TRes IMessagePoster.Producer<TRes>(string channel, string title)
        {
            DoProducer(channel, title, null);
            return default;
        }


        Task<string> IMessagePoster.ProducerAsync(string channel, string title, string content)
        {
            var id = DoProducer(channel, title, null);
            return Task.FromResult(id);
        }

        Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, JsonHelper.SerializeObject(content));
            return Task.FromResult(default(TRes));
        }
        Task IMessagePoster.ProducerAsync<TArg>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, null);
            return Task.CompletedTask;
        }

        Task<TRes> IMessagePoster.ProducerAsync<TRes>(string channel, string title)
        {
            DoProducer(channel, title, null);
            return Task.FromResult(default(TRes));
        }

    */
