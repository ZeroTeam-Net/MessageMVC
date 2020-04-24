using Agebull.Common;
using Agebull.Common.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     Kafka消息发布
    /// </summary>
    internal class KafkaPoster : MessagePostBase, IMessagePoster, IFlowMiddleware, IHealthCheck
    {
        #region IHealthCheck

        async Task<HealthInfo> IHealthCheck.Check()
        {
            var info = new HealthInfo
            {
                ItemName = nameof(KafkaPoster),
                Items = new List<HealthItem>()
            };


            info.Items.Add(await ProduceTest());
            info.Items.Add(ConsumerTest());
            return info;
        }

        private static async Task<HealthItem> ProduceTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Produce"
            };
            try
            {
                DateTime start = DateTime.Now;
                var re = await producer.ProduceAsync(KafkaOption.Instance.TestTopic,
                    new Message<Null, string> { Value = "HealthCheck" });

                item.Value = (DateTime.Now - start).TotalMilliseconds;
                item.Details = re.Status.ToString();
                switch (re.Status)
                {
                    case PersistenceStatus.NotPersisted:
                        item.Level = 0;
                        break;
                    default:
                        if (item.Value < 10)
                            item.Level = 5;
                        else if (item.Value < 100)
                            item.Level = 4;
                        else if (item.Value < 500)
                            item.Level = 3;
                        else if (item.Value < 3000)
                            item.Level = 2;
                        else item.Level = 1;
                        break;
                }

            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }

        private HealthItem ConsumerTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Consumer"
            };
            try
            {
                var config = KafkaOption.Instance.CopyConsumer();
                config.GroupId = ZeroAppOption.Instance.AppName;
                var builder = new ConsumerBuilder<Ignore, string>(config);
                using var consumer = builder.Build();
                consumer.Subscribe(KafkaOption.Instance.TestTopic);

                DateTime start = DateTime.Now;
                var re = consumer.Consume(new TimeSpan(0, 0, 3));

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
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }
        #endregion

        #region IFlowMiddleware 

        StationStateType State;

        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType IMessagePoster.State { get => State; set => State = value; }

        private static IProducer<Null, string> producer;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(KafkaPoster);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        /// <summary>
        /// 关闭
        /// </summary>
        void IFlowMiddleware.Start()
        {
            producer = new ProducerBuilder<Null, string>(KafkaOption.Instance.Producer).Build();
            State = StationStateType.Run;
            tokenSource = new CancellationTokenSource();
            _ = AsyncPostQueue();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void IFlowMiddleware.Close()
        {
            tokenSource?.Cancel();
            tokenSource.Dispose();
            tokenSource = null;
            State = StationStateType.Closed;
            producer?.Dispose();
            producer = null;
        }

        #endregion

        #region 消息可靠性

        /// <summary>
        /// 单例
        /// </summary>
        public static KafkaPoster Instance = new KafkaPoster();

        private class KafkaQueueItem
        {
            public string ID { get; set; }
            public string Topic { get; set; }
            public string Message { get; set; }
            public string FileName { get; set; }
            public int Try { get; set; }
        }

        private static readonly ConcurrentQueue<KafkaQueueItem> queues = new ConcurrentQueue<KafkaQueueItem>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private CancellationTokenSource tokenSource;

        private async Task AsyncPostQueue()
        {
            await Task.Yield();
            //还原发送异常文件
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "redis");
            var isFailed = ReQueueErrorMessage(path);

            Logger.Information("异步消息投递已启动");
            while (ZeroFlowControl.IsAlive)
            {
                if (isFailed)
                {
                    await Task.Delay(1000);
                    isFailed = true;
                }
                if (queues.Count == 0)
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

                while (queues.TryDequeue(out KafkaQueueItem item))
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
            Logger.Information(() => $"载入发送错误消息,总数{files.Count}");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    queues.Enqueue(SmartSerializer.ToObject<KafkaQueueItem>(json));
                }
                catch (Exception ex)
                {
                    Logger.Warning(() => $"{file} : 消息载入错误.{ex.Message}");
                }
            }

            return true;
        }

        private async Task<bool> DoPost(ILogger logger, string path, KafkaQueueItem item)
        {
            var State = false;
            try
            {
                logger.Trace(() => $"[异步消息投递] 正在投递消息.{KafkaOption.Instance.BootstrapServers}");
                var ret = await producer.ProduceAsync(item.Topic, new Message<Null, string>
                {
                    Value = item.Message
                });
                logger.Trace(() => $"[异步消息投递] 投递结果:{ret.Status}");
                queues.TryDequeue(out _);
                State = true;
            }
            catch (Exception ex)
            {
                logger.Warning(() => $"[异步消息投递] {item.ID} 发送失败.{ex.Message}");
            }
            if (State)
            {
                if (item.FileName != null)
                {
                    try
                    {
                        logger.Trace(() => $"[异步消息投递] {item.ID} 发送成功,删除备份文件,{item.FileName}");
                        File.Delete(item.FileName);
                    }
                    catch
                    {
                        logger.Warning(() => $"[异步消息投递] {item.ID} 发送成功,删除备份文件失败,{item.FileName}");
                    }
                }
                else
                {
                    logger.Information(() => $"[异步消息投递] {item.ID} 发送成功");
                }
                return true;
            }
            queues.Enqueue(item);
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

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            message.Offline();
            queues.Enqueue(new KafkaQueueItem
            {
                ID = message.ID,
                Topic = message.Topic,
                Message = SmartSerializer.SerializeMessage(message)
            });
            semaphore.Release();
            message.RealState = MessageState.AsyncQueue;
            LogRecorder.MonitorDetails(() =>"[KafkaPoster.Post] 消息已投入发送队列,将在后台静默发送直到成功");
            return Task.FromResult<IMessageResult>(null);//直接使用状态
        }
        #endregion
    }
}

/*
 
        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public TRes Producer<TArg, TRes>(string topic, string title, TArg content)
        {
            var (success, result) = ProducerInner(topic, title, JsonHelper.SerializeObject(content)).Result;
            return success ? JsonHelper.DeserializeObject<TRes>(result) : default;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public void Producer<TArg>(string topic, string title, TArg content)
        {
            ProducerInner(topic, title, JsonHelper.SerializeObject(content)).Wait();
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        public TRes Producer<TRes>(string topic, string title)
        {
            var (success, result) = ProducerInner(topic, title, null).Result;
            return success ? JsonHelper.DeserializeObject<TRes>(result) : default;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public string Producer(string topic, string title, string content)
        {
            var (success, result) = ProducerInner(topic, title, content).Result;
            return result;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public async Task<TRes> ProducerAsync<TArg, TRes>(string topic, string title, TArg content)
        {
            var (success, result) = await ProducerInner(topic, title, JsonHelper.SerializeObject(content));
            return success ? JsonHelper.DeserializeObject<TRes>(result) : default;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public Task ProducerAsync<TArg>(string topic, string title, TArg content)
        {
            return ProducerInner(topic, title, JsonHelper.SerializeObject(content));
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        public async Task<TRes> ProducerAsync<TRes>(string topic, string title)
        {
            var (success, result) = await ProducerInner(topic, title, null);
            return success ? JsonHelper.DeserializeObject<TRes>(result) : default;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public async Task<string> ProducerAsync(string topic, string title, string content)
        {
            var (_, result) = await ProducerInner(topic, title, null);
            return result;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<string> IMessagePoster.ProducerAsync(IMessageItem message)
        {
            var (success, result) = await ProducerInner(message);
            return success ? default : result;
        }

        /// <param name="topic"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private Task<(bool success, string result)> ProducerInner(string topic, string title, string content)
        {
            var item = new MessageItem
            {
                Topic = topic,
                Title = title,
                Content = content
            };
            if (ZeroAppOption.Instance.EnableLinkTrace && GlobalContext.CurrentNoLazy != null)
            {
                item.Context = JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy);
            }
            return ProducerInner(item);
        }

*/
