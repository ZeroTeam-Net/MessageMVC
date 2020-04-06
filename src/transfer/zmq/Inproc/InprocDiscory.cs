using System;
using System.Reflection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// 默认网络传输对象发现
    /// </summary>
    public class InprocDiscory : ITransportDiscory
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
            var sa = type.GetCustomAttribute<InprocAttribute>();
            if (sa != null)
            {
                MessagePoster.RegistPoster<InprocPoster>(sa.Name);
                name = sa.Name;
                return InprocTransportBuilder;
            }
            return TransportDiscover.DiscoryNetTransport(type, out name);
        }
    }
}