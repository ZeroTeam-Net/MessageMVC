using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     Kafka消息发布
    /// </summary>
    public class KafkaPoster : NewtonJsonSerializeProxy, IMessagePoster, IFlowMiddleware
    {

        #region IFlowMiddleware 

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        private static IProducer<Null, string> producer;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroMiddleware.Name => "KafkaPoster";

        ConsumerConfig config = ConfigurationManager.Get<ConsumerConfig>("MessageMVC:Kafka");

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => 0;

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(KafkaPoster));
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Start()
        {
            producer = new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers
            }).Build();
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
            producer?.Dispose();
            producer = null;
        }

        #endregion


        #region 消息可靠性

        private class KafkaQueueItem
        {
            public string ID { get; set; }
            public string Topic { get; set; }
            public string Message { get; set; }
            public string FileName { get; set; }
            public int Try { get; set; }
        }
        ILogger logger;
        private static readonly ConcurrentQueue<KafkaQueueItem> queues = new ConcurrentQueue<KafkaQueueItem>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private CancellationTokenSource tokenSource;

        private async Task AsyncPostQueue()
        {
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
                if (queues.Count == 0)
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

                while (queues.TryPeek(out KafkaQueueItem item))
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
                    queues.Enqueue(JsonHelper.DeserializeObject<KafkaQueueItem>(json));
                }
                catch (Exception ex)
                {
                    logger.Warning(() => $"{file} : 消息载入错误.{ex.Message}");
                }
            }

            return true;
        }

        private async Task<bool> DoPost(ILogger logger, string path, KafkaQueueItem item)
        {
            var state = false;
            try
            {
                logger.Trace(() => $"[异步消息投递] 正在投递消息.{config.BootstrapServers}");
                var ret = await producer.ProduceAsync(item.Topic, new Message<Null, string>
                {
                    Value = item.Message
                });
                logger.Trace(() => $"[异步消息投递] 投递结果:{ret.Status}");
                queues.TryDequeue(out _);
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

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public Task<IMessageResult> Post(IInlineMessage message)
        {
            var item = new KafkaQueueItem
            {
                ID = message.ID,
                Topic = message.Topic,
                Message = ToString(message, false)
            };
            LogRecorder.MonitorTrace("[KafkaPoster.Post] 消息已投入发送队列,将在后台静默发送直到成功");
            queues.Enqueue(item);
            semaphore.Release();
            message.State = MessageState.Accept;
            message.RuntimeStatus = ApiResultHelper.Helper.Waiting;
            return Task.FromResult(message.ToMessageResult());
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
