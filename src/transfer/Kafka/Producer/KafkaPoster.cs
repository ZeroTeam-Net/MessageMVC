using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Confluent.Kafka;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     Kafka消息发布
    /// </summary>
    public class KafkaPoster : IMessagePoster, IFlowMiddleware
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

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => 0;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            ConsumerConfig config = ConfigurationManager.Get<ConsumerConfig>("MessageMVC:Kafka");
            producer = new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers
            }).Build();
            State = StationStateType.Run;
        }

        /// <summary>
        ///     关闭
        /// </summary>
        void IFlowMiddleware.Close()
        {
            State = StationStateType.Closed;
            producer?.Dispose();
            producer = null;
        }
        #endregion

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task<(MessageState state, string result)> Post(IMessageItem message)
        {
            try
            {
                if (producer == null)
                {
                    return (MessageState.NoSupper, null);
                }
                var msg = new Message<Null, string>
                {
                    Value = JsonHelper.SerializeObject(message)
                };
                var ret = await producer.ProduceAsync(message.Topic, msg);
                return ret.Status == PersistenceStatus.Persisted
                    ? (MessageState.Success, ret.Value)
                    : (MessageState.NetError, null);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "Kafka Production<Error>");
                return (MessageState.NetError, null);
            }
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
