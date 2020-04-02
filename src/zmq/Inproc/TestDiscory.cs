using System;
using System.Reflection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 测试对象发现
    /// </summary>
    public class TestDiscory : ITransportDiscory
    {
        private INetTransfer InprocTransportBuilder(string name) => new InporcConsumer();

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        Func<string, INetTransfer> ITransportDiscory.DiscoryNetTransport(Type type, out string name)
        {
            {
                var ia = type.GetCustomAttribute<InprocAttribute>();
                if (ia != null)
                {
                    name = ia.Name;
                    return InprocTransportBuilder;
                }
            }
            #region Api
            {
                var sa = type.GetCustomAttribute<ServiceAttribute>();
                if (sa != null)
                {
                    name = sa.Name;
                    return InprocTransportBuilder;
                }
            }
            #endregion

            #region MQ
            {
                var ca = type.GetCustomAttribute<ConsumerAttribute>();
                if (ca != null)
                {
                    name = ca.Topic;
                    return InprocTransportBuilder;
                }
            }
            #endregion

            #region NetEvent
            {
                var na = type.GetCustomAttribute<NetEventAttribute>();
                if (na != null)
                {
                    name = na.Name;
                    return InprocTransportBuilder;
                }
            }
            #endregion
            throw new Exception($"控制器{type.FullName},缺少必要的服务类型声明,请使用ServiceAttribute\\ConsumerAttribute\\NetEventAttribute之一特性声明为Api服务\\消息队列订阅\\分布式事件处理");
        }
    }
}