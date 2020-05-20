using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     表示ZeroRpc服务
    /// </summary>
    public class ZeroRpcAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public ZeroRpcAttribute(string name)
        {
            ServiceName = name;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return new ZeroRpcReceiver();
        }
    }

    /// <summary>
    ///     表示ZeroRpc事件
    /// </summary>
    public class ZeroEventAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public ZeroEventAttribute(string name)
        {
            ServiceName = name;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return new ZeroEventReceiver();
        }
    }
}
