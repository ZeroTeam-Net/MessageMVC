using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     表示Kafka消费者
    /// </summary>
    public class KafkaAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public KafkaAttribute(string name)
        {
            ServiceName = name;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return new KafkaConsumer();
        }
    }
}
