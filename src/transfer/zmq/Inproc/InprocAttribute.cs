using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 使用ZeroMQ实现的进程内通讯
    /// </summary>
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
}