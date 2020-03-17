using Agebull.Common.Configuration;
using Agebull.Common.Logging;
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
            try
            {
                var tm = new TimeSpan(0, 0, 3);
                while (!token.IsCancellationRequested)
                {
                    var cr = consumer.Consume(tm);
                    Console.WriteLine("...");
                    if (cr == null)
                    {
                        Console.WriteLine("Empty");
                        continue;
                    }
                    ApiExecuter.OnMessagePush(Station, cr.Value);
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
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
        ConsumerBuilder<Ignore, string> builder;
        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        public void LoopBegin()
        {
            builder = new ConsumerBuilder<Ignore, string>(config);
            consumer = builder.Build();
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
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            consumer.Close();
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
