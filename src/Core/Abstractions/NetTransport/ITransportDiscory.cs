using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 自定义网络传输对象发现
    /// </summary>
    public interface ITransportDiscory
    {
        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="type">控制器类型</param>
        /// <param name="name">发现的服务名称</param>
        /// <returns>传输对象构造器</returns>
        Func<string, INetTransport> DiscoryNetTransport(Type type,out string name);
    }
}