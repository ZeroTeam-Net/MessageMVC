using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 自定义消息接收对象发现
    /// </summary>
    public interface IReceiverDiscory
    {
        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        Func<string, IMessageReceiver> DiscoryNetTransport(Type type, out string name);
    }
}