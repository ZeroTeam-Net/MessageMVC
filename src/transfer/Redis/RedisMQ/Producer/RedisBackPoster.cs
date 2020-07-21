using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using CSRedis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis后台发布
    /// </summary>
    internal class RedisBackPoster : IFlowMiddleware, IHealthCheck
    {
        #region IHealthCheck

        async Task<HealthInfo> IHealthCheck.Check()
        {
            var info = new HealthInfo
            {
                ItemName = nameof(RedisBackPoster),
                Items = new List<HealthItem>()
            };

            info.Items.Add(await SetTest());
            info.Items.Add(await GetTest());
            info.Items.Add(await DelTest());
            return info;
        }

        private async Task<HealthItem> SetTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Set"
            };
            try
            {
                DateTime start = DateTime.Now;
                if (!await client.SetAsync("_health_", "c", 10))
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    item.Level = 0;
                }
                else
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (item.Value < 10)
                        item.Level = 5;
                    else if (item.Value < 100)
                        item.Level = 4;
                    else if (item.Value < 500)
                        item.Level = 3;
                    else if (item.Value < 3000)
                        item.Level = 2;
                    else item.Level = 1;
                }
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }

        private async Task<HealthItem> GetTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Get"
            };
            try
            {
                DateTime start = DateTime.Now;
                if (await client.GetAsync("_health_") != "c")
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    item.Level = 0;
                }
                else
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (item.Value < 10)
                        item.Level = 5;
                    else if (item.Value < 100)
                        item.Level = 4;
                    else if (item.Value < 500)
                        item.Level = 3;
                    else if (item.Value < 3000)
                        item.Level = 2;
                    else item.Level = 1;
                }
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }
        private async Task<HealthItem> DelTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Del"
            };
            try
            {
                DateTime start = DateTime.Now;
                if (await client.DelAsync("_health_") != 1)
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    item.Level = 0;
                }
                else
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (item.Value < 10)
                        item.Level = 5;
                    else if (item.Value < 100)
                        item.Level = 4;
                    else if (item.Value < 500)
                        item.Level = 3;
                    else if (item.Value < 3000)
                        item.Level = 2;
                    else item.Level = 1;
                }
                await client.DelAsync("_health_");
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }

        #endregion

        #region 消息可靠性

        private static readonly ConcurrentQueue<RedisQueueItem> redisQueues = new ConcurrentQueue<RedisQueueItem>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private CancellationTokenSource tokenSource;

        private async Task AsyncPostQueue()
        {
            await Task.Yield();
            try
            {
                client.Ping();
            }
            catch (RedisClientException ex)
            {
                Logger.Error(ex.Message);
            }
            await Task.Yield();
            //还原发送异常文件
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "redis", "queue");
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

                while (redisQueues.TryDequeue(out RedisQueueItem item))
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
                    redisQueues.Enqueue(SmartSerializer.ToObject<RedisQueueItem>(json));
                }
                catch (Exception ex)
                {
                    Logger.Warning(() => $"{file} : 消息载入错误.{ex.Message}");
                }
            }

            return true;
        }


        private async Task<bool> DoPost(ILogger logger, string path, RedisQueueItem item)
        {
            var state = false;
            try
            {
                if (item.IsEvent)
                {
                    await client.PublishAsync(item.Channel, item.Message);
                    item.Step = 3;
                }
                else switch (item.Step)
                    {
                        case 0:
                            await client.SetAsync($"msg:{item.Channel}:{item.ID}", item.Message);
                            item.Step = 1;
                            break;
                        case 1:
                            await client.LPushAsync($"msg:{item.Channel}", item.ID);
                            item.Step = 2;
                            break;
                        case 2:
                            await client.PublishAsync(item.Channel, item.ID);
                            item.Step = 3;
                            break;
                    }
                state = true;
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

        #region 发布消息

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="isEvent">是否事件</param>
        /// <returns></returns>
        internal static Task<IMessageResult> Post(IInlineMessage message, bool isEvent)
        {
            message.Offline();
            redisQueues.Enqueue(new RedisQueueItem
            {
                ID = message.ID,
                IsEvent = isEvent,
                Channel = message.Topic,
                Message = SmartSerializer.SerializeMessage(message)
            });
            semaphore.Release();
            message.RealState = MessageState.AsyncQueue;
            FlowTracer.MonitorDetails("[RedisBackPoster.Post] 消息已投入发送队列,将在后台静默发送直到成功");
            return Task.FromResult<IMessageResult>(null);//直接使用状态
        }

        #endregion

        #region IFlowMiddleware

        /// <summary>
        /// 单例
        /// </summary>
        public static RedisBackPoster Instance = new RedisBackPoster();

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(RedisBackPoster);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        private CSRedisClient client;

        /// <summary>
        /// 日志器
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(GetType().GetTypeName());
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Open()
        {
            Logger.Information("RedisBackPoster >>> Start");
            client = new CSRedisClient(RedisOption.Instance.ConnectionString);
            tokenSource = new CancellationTokenSource();
            return AsyncPostQueue();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Close()
        {
            Logger.Information("RedisBackPoster >>> Close");
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        Task ILifeFlow.Destory()
        {
            Logger.Information("RedisBackPoster >>> Check");

            client.Dispose();
            return Task.CompletedTask;
        }

        #endregion

    }
}
