﻿using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个远程服务(RPC或HTTP)传输对象
    /// </summary>
    public interface IServiceTransfer : INetTransfer
    {
    }

    /// <summary>
    /// 表明是一个服务控制器
    /// </summary>
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public ServiceAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }
}