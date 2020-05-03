using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// API配置过滤器
    /// </summary>
    public class ApiOptionAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="option"></param>
        public ApiOptionAttribute(ApiOption option)
        {
            Option = option;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public ApiOption Option { get; }
    }
}
