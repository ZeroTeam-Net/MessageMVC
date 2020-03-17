using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// API对应页面的特性
    /// </summary>
    public class ApiPageAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="pageUrl">页面</param>
        public ApiPageAttribute(string pageUrl)
        {
            PageUrl = pageUrl;
        }
        /// <summary>
        /// 页面
        /// </summary>
        public string PageUrl { get; }
    }
}
