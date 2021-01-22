using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.RedisMQ
{

    /// <summary>
    ///     表示Redis事件
    /// </summary>
    public class RedisQueueAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public RedisQueueAttribute(string name)
        {
            ServiceName = name;
        }
        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return new CSRedisQueueReceiver();
        }
    }
}
