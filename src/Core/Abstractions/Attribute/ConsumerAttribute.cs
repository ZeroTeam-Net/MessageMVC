using Agebull.Common.Ioc;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// 表示一个消息队列消费者
    /// </summary>
    public class ConsumerAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="topic"></param>
        public ConsumerAttribute(string topic)
        {
            ServiceName = topic;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return DependencyHelper.GetService<IMessageConsumer>();
        }
    }
}