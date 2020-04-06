using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个分布式事件处理服务
    /// </summary>
    public class NetEventAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="event"></param>
        public NetEventAttribute(string @event)
        {
            Name = @event;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }
}