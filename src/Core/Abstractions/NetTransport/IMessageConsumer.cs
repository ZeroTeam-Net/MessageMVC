using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个消息队列消费对象
    /// </summary>
    public interface IMessageConsumer : INetTransport
    {
    }

    /// <summary>
    /// 表示一个消息队列消费者
    /// </summary>
    public class ConsumerAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="topic"></param>
        public ConsumerAttribute(string topic)
        {
            Topic = topic;
        }
        /// <summary>
        /// 消息节点
        /// </summary>
        public string Topic { get; }
    }
}