using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 路由名称
    /// </summary>
    [Obsolete("由于NetCore3.1已取消对等名称的特性,请使用RouteAttribute以保证未来的可迁移性")]
    public class RoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public RoutePrefixAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }
}
