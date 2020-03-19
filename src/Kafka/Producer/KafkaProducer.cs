using System;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Confluent.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;
using ErrorCode = ZeroTeam.MessageMVC.ZeroApis.ErrorCode;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     消息发布
    /// </summary>
    internal class KafkaProducer : IMessageProducer
    {
        #region Producer 

        private static IProducer<Null, string> producer;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            IocHelper.AddSingleton<IMessageProducer, KafkaProducer>();
            ConsumerConfig config = ConfigurationManager.Get<ConsumerConfig>("Kafka");
            producer = new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers
            }).Build();
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static IApiResult PublishInner(string topic, string title, string content)
        {
            var task = PublishAsyncInner(topic, title, content);
            task.Wait();
            return task.Result;
        }

        /// <param name="topic"></param>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static async Task<IApiResult> PublishAsyncInner(string topic, string title, string content)
        {
            try
            {
                var item = new MessageItem
                {
                    Topic = topic,
                    Title = title,
                    Content = content
                };
                var msg = new Message<Null, string> { Value = JsonHelper.SerializeObject(item) };
                var ret = await producer.ProduceAsync(topic, msg);
                return ret.Status == PersistenceStatus.Persisted
                    ? ApiResult.Succees()
                    : new ApiResult
                    {
                        Success = false,
                        Status = new OperatorStatus
                        {
                            Code = ErrorCode.Ignore
                        }
                    };
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "Kafka Production<Error>");
                return new ApiResult
                {
                    Success = false,
                    Status = new OperatorStatus
                    {
                        Code = ErrorCode.LocalException,
                        InnerMessage = $"Kafka Production<Error> : {e.Message}"
                    }
                };
            }
        }

        public IApiResult Producer(string topic, string title, string content)
        {
            return PublishInner(topic, title, content);
        }

        public IApiResult Producer<T>(string topic, string title, T content)
        {
            return PublishInner(topic, title, JsonHelper.SerializeObject(content));
        }

        public Task<IApiResult> ProducerAsync(string topic, string title, string content)
        {
            return PublishAsyncInner(topic, title, content);
        }

        public Task<IApiResult> ProducerAsync<T>(string topic, string title, T content)
        {
            return PublishAsyncInner(topic, title, JsonHelper.SerializeObject(content));
        }

        #endregion
    }
}
