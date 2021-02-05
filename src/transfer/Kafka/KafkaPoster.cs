using Agebull.Common.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     Kafka消息发布
    /// </summary>
    internal sealed class KafkaPoster : BackgroundPoster<QueueItem>, ILifeFlow, IZeroDiscover, IHealthCheck
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static KafkaPoster Instance = new KafkaPoster();

        /// <summary>
        /// 征集周期管理器
        /// </summary>
        protected override ILifeFlow LifeFlow => this;

        public KafkaPoster()
        {
            Name = nameof(KafkaPoster);
        }
        protected override TaskCompletionSource<IMessageResult> DoPost(QueueItem item)
        {
            Logger.Trace(() => $"[异步消息投递] {item.ID} 正在投递消息.{KafkaOption.Instance.BootstrapServers}");
            var res = new TaskCompletionSource<IMessageResult>();
            producer.ProduceAsync(item.Topic, new Message<byte[], string>
            {
                Key = JsonSerializer.SerializeToUtf8Bytes(new MessageItem
                {
                    ID = item.Message.ID,
                    Method = item.Message.Method,
                    TraceInfo = item.Message.TraceInfo,
                    Context = item.Message.Context,
                    User = item.Message.User,
                }),
                Value = item.Message.Argument,
                //Headers = headers
            })
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.Trace(() => $"[异步消息投递] {item.ID} 发生异常({task.Exception.Message})");
                    res.TrySetResult(new MessageResult
                    {
                        ID = item.Message.ID,
                        Trace = item.Message.TraceInfo,
                        State = MessageState.NetworkError
                    });
                }
                else if (task.IsFaulted)
                {
                    Logger.Trace(() => $"[异步消息投递] {item.ID} 操作取消");

                    res.TrySetResult(new MessageResult
                    {
                        ID = item.Message.ID,
                        Trace = item.Message.TraceInfo,
                        State = MessageState.Cancel
                    });
                }
                else
                {
                    Logger.Trace(() => $"[异步消息投递] {item.ID} 投递结果:{task.Result.ToJson()}");

                    res.TrySetResult(new MessageResult
                    {
                        ID = item.Message.ID,
                        Trace = item.Message.TraceInfo,
                        State = task.Result.Status == PersistenceStatus.NotPersisted ? MessageState.Success : MessageState.NetworkError
                    });
                }
            });
            return res;
        }


        #region IFlowMiddleware 

        internal IProducer<byte[], string> producer;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(KafkaPoster);

        /// <summary>
        /// 发现期间开启任务
        /// </summary>
        Task IZeroDiscover.Discovery()
        {
            producer = new ProducerBuilder<byte[], string>(KafkaOption.Instance.Producer).Build();
            AsyncPost = KafkaOption.Instance.Message.AsyncPost;
            DoStart();
            Logger.Information($"{Name}已开启");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        async Task ILifeFlow.Destroy()
        {
            await Destroy();
            try
            {
                producer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            producer = null;
            RecordLog(LogLevel.Information, $"{Name}已关闭");
        }

        #endregion

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

        private async Task<HealthItem> ProduceTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Produce"
            };
            try
            {
                DateTime start = DateTime.Now;
                var re = await producer.ProduceAsync(KafkaOption.Instance.Message.TestTopic,
                    new Message<byte[], string> { Value = "HealthCheck" });

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
                var config = KafkaOption.Instance.Consumer;
                config.GroupId = ZeroAppOption.Instance.AppName;
                var builder = new ConsumerBuilder<Ignore, string>(config);
                using var consumer = builder.Build();
                consumer.Subscribe(KafkaOption.Instance.Message.TestTopic);

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
    }
}
/*// <summary>
/// 
/// </summary>
internal class KafkaKey
{
    /// <summary>
    /// 消息标识
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// 方法
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    ///     跟踪信息
    /// </summary>
    public TraceInfo Trace { get; set; }

    /// <summary>
    /// 上下文信息
    /// </summary>
    public Dictionary<string, string> Context { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public Dictionary<string, string> User { get; set; }

}

        //var headers = new Headers
        //{
        //    { "zid", Encoding.ASCII.GetBytes(item.Message.ID) },
        //    { "method", Encoding.ASCII.GetBytes(item.Message.Method) }
        //};
        //if (item.Message.Trace != null)
        //    headers.Add("trace", JsonSerializer.SerializeToUtf8Bytes(item.Message.Trace));
        //if (item.Message.Context != null)
        //    headers.Add("ctx", JsonSerializer.SerializeToUtf8Bytes(item.Message.Context));
        //if (item.Message.User != null)
        //    headers.Add("ctx", JsonSerializer.SerializeToUtf8Bytes(item.Message.User));
*/

