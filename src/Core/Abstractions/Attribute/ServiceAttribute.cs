using Agebull.Common.Ioc;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表明是一个服务控制器
    /// </summary>
    public class ServiceAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public ServiceAttribute(string name)
        {
            ServiceName = name;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return DependencyHelper.GetService<IServiceReceiver>();
        }
    }
}