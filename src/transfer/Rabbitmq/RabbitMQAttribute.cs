using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    ///     表示RabbitMQ消费者
    /// </summary>
    public class RabbitMQConsumerAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="queue">队列名称</param>
        public RabbitMQConsumerAttribute(string queue)
        {
            Queue = queue;
        }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string Queue { get; }

        string IReceiverGet.ServiceName => Queue;

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return new RabbitMQConsumer();
        }
    }

}
