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
        /// <param name="service">服务</param>
        public ServiceAttribute(string service)
        {
            ServiceName = service;
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="service">服务</param>
        /// <param name="prefix">接口前缀</param>
        public ServiceAttribute(string service,string prefix)
        {
            ServiceName = service;
            ApiPrefix = prefix;
        }

        /// <summary>
        /// 接口前缀
        /// </summary>
        public string ApiPrefix { get; }


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