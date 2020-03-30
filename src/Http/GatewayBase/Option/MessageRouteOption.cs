using System.Collections.Generic;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由配置
    /// </summary>
    public class MessageRouteOption
    {
        /// <summary>
        /// 启用文件上传
        /// </summary>
        public bool EnableFormFile { get; set; }

        /// <summary>
        /// 启用全局上下文
        /// </summary>
        public bool EnableGlobalContext { get; set; }

        /// <summary>
        /// 启用身份令牌
        /// </summary>
        public bool EnableAuthToken { get; set; }

        /// <summary>
        /// 启用UserAgent
        /// </summary>
        public bool EnableUserAgent { get; set; }

        /// <summary>
        /// 启用快速调用,即直接使用ApiExecuter
        /// </summary>
        public bool FastCall { get; set; }


        /// <summary>
        /// 启用HttpHeader
        /// </summary>
        public bool EnableHttpHeader { get; set; }

        /// <summary>
        /// 特殊URL取第几个路径作为服务名称的映射表
        /// </summary>
        /// <remarks>
        /// 当启用NGINX代理时,NGINX可能会增加一级节点,而导致默认第1个路径作为服务名称失效
        /// </remarks>
        public Dictionary<string,int> HostPaths { get; set; }
    }

}