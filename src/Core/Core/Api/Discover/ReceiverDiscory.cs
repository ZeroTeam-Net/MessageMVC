using Agebull.Common.Ioc;
using System;
using System.Reflection;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 默认消息接收对象发现
    /// </summary>
    public class ReceiverDiscover : IReceiverDiscover
    {
        private static IMessageReceiver RpcTransportBuilder(string name) => DependencyHelper.Create<IServiceReceiver>();
        private static IMessageReceiver ConsumerTransportBuilder(string name) => DependencyHelper.Create<IMessageConsumer>();
        private static IMessageReceiver NetEventTransportBuilder(string name) => DependencyHelper.Create<INetEvent>();

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        Func<string, IMessageReceiver> IReceiverDiscover.DiscoverNetTransport(Type type, out string name)
        {
            return DiscoverNetTransport(type, out name);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        public static Func<string, IMessageReceiver> DiscoverNetTransport(Type type, out string name)
        {
            #region Api
            var sa = type.GetCustomAttribute<ServiceAttribute>();
            if (sa != null)
            {
                name = sa.Name;
                return RpcTransportBuilder;
            }
            #endregion

            #region MQ
            var ca = type.GetCustomAttribute<ConsumerAttribute>();
            if (ca != null)
            {
                name = ca.Topic;
                return ConsumerTransportBuilder;
            }
            #endregion

            #region NetEvent
            var na = type.GetCustomAttribute<NetEventAttribute>();
            if (na != null)
            {
                name = na.Name;
                return NetEventTransportBuilder;
            }
            #endregion

            name = null;
            return null;
            //throw new Exception($"控制器{type.FullName},缺少必要的服务类型声明,请使用ServiceAttribute\\ConsumerAttribute\\NetEventAttribute之一特性声明为Api服务\\消息队列订阅\\分布式事件处理");
        }
    }
}