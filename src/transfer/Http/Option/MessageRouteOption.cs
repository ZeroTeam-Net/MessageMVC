using Agebull.Common.Configuration;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     路由配置
    /// </summary>
    internal class MessageRouteOption
    {
        /// <summary>
        /// 标识内部调用的代理名称
        /// </summary>
        public const string AgentName = "MessageMVC";

        /// <summary>
        /// 启用文件上传
        /// </summary>
        public bool EnableFormFile { get; set; }

        /// <summary>
        /// 特殊URL取第几个路径作为服务名称的映射表
        /// </summary>
        /// <remarks>
        /// 当启用NGINX代理时,NGINX可能会增加一级节点,而导致默认第1个路径作为服务名称失效
        /// </remarks>
        public Dictionary<string, int> HostPaths { get; set; }

        /// <summary>
        /// 选项
        /// </summary>
        public static MessageRouteOption Instance = new MessageRouteOption
        {
            HostPaths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
        };


        static MessageRouteOption()
        {
            ConfigurationHelper.RegistOnChange<MessageRouteOption>("Http:Message", Instance.Load, true);
        }

        void Load(MessageRouteOption option)
        {
            if (option.HostPaths != null)
                Instance.HostPaths = option.HostPaths;
            Instance.EnableFormFile = option.EnableFormFile;
        }
    }
}