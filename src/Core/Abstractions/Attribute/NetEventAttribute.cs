using Agebull.Common.Ioc;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个分布式事件处理服务
    /// </summary>
    public class NetEventAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="event"></param>
        public NetEventAttribute(string @event)
        {
            ServiceName = @event;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return DependencyHelper.GetService<INetEvent>();
        }
    }
}