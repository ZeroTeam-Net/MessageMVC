﻿using System;
using System.Reflection;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    public class InprocAttribute : Attribute
    {

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public InprocAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }

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
                MessageProducer.Register(sa.Name, ZmqProducer.Instance);
                name = sa.Name;
                return InprocTransportBuilder;
            }
            return TransportDiscory.DiscoryNetTransport(type,out name);
        }
    }
}