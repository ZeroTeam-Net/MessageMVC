using Agebull.Common.Ioc;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个消息队列消费者
    /// </summary>
    public class ReceiverAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造一个泛接收器
        /// </summary>
        /// <param name="topic">服务名称</param>
        public ReceiverAttribute(string topic)
        {
            ServiceName = topic;
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="topic">服务名称</param>
        /// <param name="receiverName">接收器名称</param>
        public ReceiverAttribute(string receiverName, string topic)
        {
            ServiceName = topic;
            ReceiverName = receiverName;
        }
        /// <summary>
        /// 消息节点
        /// </summary>
        public string ReceiverName { get; }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return ReceiverName == null ? null : DependencyHelper.GetService<IMessageReceiver>(ReceiverName);
        }
    }
}