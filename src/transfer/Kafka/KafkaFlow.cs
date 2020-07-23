using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Kafka;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     RabbitMQ流程处理
    /// </summary>
    internal class KafkaFlow : IFlowMiddleware, IHealthCheck
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static KafkaFlow Instance = new KafkaFlow();

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

        internal IProducer<Null, string> producer;

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
        Task ILifeFlow.Open()
        {
            producer = new ProducerBuilder<Null, string>(KafkaOption.Instance.Producer).Build();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Close()
        {
            producer?.Dispose();
            producer = null;
            return Task.CompletedTask;
        }

        #endregion

    }
}
