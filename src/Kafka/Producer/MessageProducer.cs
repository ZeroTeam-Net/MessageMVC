using System;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Confluent.Kafka;
using ZeroTeam.MessageMVC.PubSub;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     消息发布
    /// </summary>
    public static class MessageProducer
    {
        #region Sync

        /// <summary>
        /// 发送广播
        /// </summary>

        /// <param name="topic"></param>
        /// <param name="sub"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool Publish<T>(string topic, string sub, T content)
            where T : class
        {
            return PublishInner(topic, sub, JsonHelper.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="content"></param>

        /// <returns></returns>
        public static bool Publish<T>(string topic, T content) where T : class
        {
            return PublishInner(topic, null, JsonHelper.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>

        /// <returns></returns>
        public static bool Publish(string topic)
        {
            return PublishInner(topic, null, null);
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="argument"></param>

        /// <returns></returns>
        public static bool Publish(string topic, string argument)
        {
            return PublishInner(topic, null, argument);
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static bool Publish(string topic, string command, string argument)
        {
            return PublishInner(topic, command, argument);
        }
        #endregion

        #region Async


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="content"></param>

        /// <returns></returns>
        public static Task<bool> PublishAsync<T>(string topic, T content) where T : class
        {
            return PublishAsyncInner(topic, null, JsonHelper.SerializeObject(content));
        }

        /// <summary>
        /// 发送广播
        /// </summary>

        /// <param name="topic"></param>
        /// <param name="command"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Task<bool> PublishAsync<T>(string topic, string command, T content)
            where T : class
        {
            return PublishAsyncInner(topic, command, JsonHelper.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="argument"></param>

        /// <returns></returns>
        public static Task<bool> PublishAsync(string topic, string argument)
        {
            return PublishAsyncInner(topic, null, argument);
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="command"></param>
        /// <param name="argument"></param>

        /// <returns></returns>
        public static Task<bool> PublishAsync(string topic, string command, string argument)
        {
            return PublishAsyncInner(topic, command, argument);
        }
        #endregion

        #region Frames

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            IocHelper.AddSingleton<IZeroPublisher, ZeroPublisher>();
            ConsumerConfig config = ConfigurationManager.Get<ConsumerConfig>("Kafka");
            producer = new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers
            }).Build();
        }

        private static IProducer<Null, string> producer;

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static bool PublishInner(string topic, string command, string argument)
        {
            try
            {
                var frame = new PublishItem
                {
                    Topic = topic,
                    Command = command,
                    Argument = argument
                };
                var msg = new Message<Null, string> { Value = JsonHelper.SerializeObject(frame) };
                var task = producer.ProduceAsync(topic, msg);
                task.Wait();
                return task.Result.Status == PersistenceStatus.Persisted;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "Kafka Production<Error>");
                return false;
            }
        }
        /// <param name="topic"></param>
        /// <param name="command"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static async Task<bool> PublishAsyncInner(string topic, string command, string argument)
        {
            try
            {
                var frame = new PublishItem
                {
                    Topic = topic,
                    Command = command,
                    Argument = argument
                };
                var msg = new Message<Null, string> { Value = JsonHelper.SerializeObject(frame) };
                var ret = await producer.ProduceAsync(topic, msg);
                return ret.Status == PersistenceStatus.Persisted;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "Kafka Production<Error>");
                return false;
            }
        }

        #endregion
    }
}
