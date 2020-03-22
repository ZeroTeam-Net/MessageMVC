﻿using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    /// 通过ZMQ实现的进程内通讯
    /// </summary>
    public static class ZeroMQInporc
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        public static void UseZeroMQInporc(this IServiceCollection services)
        {
            services.AddTransient<IFlowMiddleware, ZmqProxy>();//ZMQ环境
            services.AddTransient<IMessageProducer, ZmqProducer>();//采用ZMQ进程内通讯生产端
            services.AddTransient<IMessageConsumer, InporcConsumer>();//采用ZMQ进程内通讯生产端
            services.AddTransient<ITransportDiscory, InprocDiscory>();//网络协议发现
        }
    }
}