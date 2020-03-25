using System;
using System.Reflection;
using Agebull.Common.Ioc;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 默认网络传输对象发现
    /// </summary>
    public class TransportDiscory : ITransportDiscory
    {
        static private INetTransfer RpcTransportBuilder(string name) => IocHelper.Create<IRpcTransfer>();
        static private INetTransfer ConsumerTransportBuilder(string name) => IocHelper.Create<IMessageConsumer>();
        static private INetTransfer NetEventTransportBuilder(string name) => IocHelper.Create<INetEvent>();

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        Func<string, INetTransfer> ITransportDiscory.DiscoryNetTransport(Type type, out string name)
        {
            return DiscoryNetTransport(type, out name);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        public static Func<string, INetTransfer> DiscoryNetTransport(Type type, out string name)
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

            throw new Exception($"控制器{type.FullName},缺少必要的服务类型声明,请使用ServiceAttribute\\ConsumerAttribute\\NetEventAttribute之一特性声明为Api服务\\消息队列订阅\\分布式事件处理");
        }
    }
}