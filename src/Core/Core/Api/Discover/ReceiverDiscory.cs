using Agebull.Common.Ioc;
using System;
using System.Reflection;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 默认消息接收对象发现
    /// </summary>
    internal static class ReceiverDiscover
    {
        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        internal static Func<string, IMessageReceiver> DiscoverNetTransport(this Type type, out string name)
        {
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is IReceiverGet receiverGet)
                {
                    name = receiverGet.ServiceName;
                    return receiverGet.Receiver;
                }
            }
            name = ZeroAppOption.Instance.ApiServiceName;
            return name => DependencyHelper.GetService<IServiceReceiver>();
        }
    }
}