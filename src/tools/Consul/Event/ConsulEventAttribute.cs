using System;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class ConsulEventAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public ConsulEventAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }
}
