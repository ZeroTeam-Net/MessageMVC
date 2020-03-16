using Agebull.Common.Configuration;
using Confluent.Kafka;
using System;
using System.Threading;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    public class KafkaConsumer : IMessageConsumer
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            config = ConfigurationManager.Get<ConsumerConfig>("Kafka");
        }
        IConsumer<Ignore, string> consumer;

        ConsumerConfig config;


        public ZeroService Station { get; set; }

        public string Name { get; set; }

        IService INetTransport.Service { get => Station; set => Station = value as ZeroService; }


        public void Dispose()
        {
        }

        public bool Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var cr = consumer.Consume(token);
                if (cr == null)
                {
                    continue;
                }
                ApiExecuter.OnMessagePush(Station, cr.Value);
            }
            return true;
        }

        /// <summary>
        /// 将要开始
        /// </summary>
        public bool Prepare()
        {
            return true;
        }

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        public void LoopBegin()
        {
            consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(Station.ServiceName);
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        public void LoopComplete()
        {
            consumer.Dispose();
        }

        /// <summary>
        /// 表示已成功接收 
        /// </summary>
        /// <returns></returns>
        public void Commit()
        {
            consumer.Commit();
        }

    }
}
