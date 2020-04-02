using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using CSRedis;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis生产者
    /// </summary>
    public class CsRedisProducer : IMessagePoster, IFlowMiddleware
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static CsRedisProducer Instance = new CsRedisProducer();


        #region 消息可靠性

        private class RedisQueueItem
        {
            public string ID { get; set; }
            public string Channel { get; set; }
            public string Message { get; set; }
            public int Step { get; set; }
            public string FileName { get; set; }
        }

        private static readonly ConcurrentQueue<RedisQueueItem> redisQueues = new ConcurrentQueue<RedisQueueItem>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private CancellationTokenSource tokenSource;

        private async Task RedisQueue()
        {
            //还原发送异常文件
            bool isFailed = false;
            var path = IOHelper.CheckPath(ZeroFlowControl.Config.DataFolder, "redis");
            var files = IOHelper.GetAllFiles(path, "*.msg");
            foreach (var file in files)
            {
                try
                {
                    isFailed = true;
                    var json = File.ReadAllText(file);
                    redisQueues.Enqueue(JsonHelper.DeserializeObject<RedisQueueItem>(json));
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
            }
            while (ZeroFlowControl.IsAlive)
            {
                if (isFailed)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    try
                    {
                        await semaphore.WaitAsync(tokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogRecorder.Exception(ex);
                        continue;
                    }
                }
                RedisQueueItem item = null;
                try
                {
                    if (!redisQueues.TryPeek(out item))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                    continue;
                }
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
                    if (item.FileName != null)
                    {
                        File.Delete(item.FileName);
                    }
                    redisQueues.TryDequeue(out item);
                    isFailed = false;
                    continue;
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
                //写入异常文件
                isFailed = true;
                try
                {
                    item.FileName = Path.Combine(path, $"{item.ID}.msg");
                    File.WriteAllText(item.FileName, JsonHelper.SerializeObject(item));
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
            }
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
        public Task<(MessageState state, string result)> Post(IMessageItem message)
        {
            var item = new RedisQueueItem
            {
                ID = message.ID,
                Channel = message.Topic,
                Message = JsonHelper.SerializeObject(message)
            };
            redisQueues.Enqueue(item);
            semaphore.Release();
            return Task.FromResult((MessageState.Accept, item.ID));
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
        int IZeroMiddleware.Level => short.MinValue;

        private CSRedisClient client;
        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            var cs = ConfigurationManager.Get("Redis")?.GetStr("ConnectionString", null);

            client = new CSRedisClient(cs);
            try
            {
                client.Ping();
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
            State = StationStateType.Run;
            tokenSource = new CancellationTokenSource();
            _ = RedisQueue();
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
