using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 路由名称
    /// </summary>
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="names"></param>
        public RouteAttribute(params string[] names)
        {
            Names = names;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string[] Names { get; }
    }

}
