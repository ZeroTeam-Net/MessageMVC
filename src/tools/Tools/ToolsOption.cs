using Agebull.Common.Configuration;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    public class ToolsOption : IZeroOption
    {
        /// <summary>
        ///     启用反向代理
        /// </summary>
        public bool EnableReverseProxy { get; set; }

        /// <summary>
        ///     反向代理服务名称映射
        /// </summary>
        public Dictionary<string, string> ReverseProxyMap { get; set; }

        /// <summary>
        ///     启用健康检查
        /// </summary>
        public bool EnableHealthCheck { get; set; }

        /// <summary>
        ///     启用第三方回执
        /// </summary>
        public bool EnableReceipt { get; set; }

        /// <summary>
        ///     启用异常消息本地重放
        /// </summary>
        public bool EnableMessageReConsumer { get; set; }

        /// <summary>
        ///     回执服务名称
        /// </summary>
        public string ReceiptService { get; set; }

        /// <summary>
        ///     回执接口方法
        /// </summary>
        public string ReceiptApi { get; set; }


        #region JWT

        /// <summary>
        ///     启用JWT令牌解析
        /// </summary>
        public bool EnableJwtToken { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        public string JwtAppSecret { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        public byte[] JwtAppSecretByte { get; set; }


        /// <summary>
        /// JWT颁发
        /// </summary>
        public string JwtIssue { get; set; }

        #endregion

        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ToolsOption Instance = new();

        const string sectionName = "MessageMVC:Tools";

        const string optionName = "扩展工具配置";

        const string supperUrl = "https://";

        /// <summary>
        /// 支持地址
        /// </summary>
        string IZeroOption.SupperUrl => supperUrl;

        /// <summary>
        /// 配置名称
        /// </summary>
        string IZeroOption.OptionName => optionName;


        /// <summary>
        /// 节点名称
        /// </summary>
        string IZeroOption.SectionName => sectionName;

        /// <summary>
        /// 是否动态配置
        /// </summary>
        bool IZeroOption.IsDynamic => false;

        void IZeroOption.Load(bool first)
        {
            ToolsOption option = ConfigurationHelper.Get<ToolsOption>(sectionName);
            if (option == null)
                return;

            EnableMessageReConsumer = option.EnableMessageReConsumer;
            EnableHealthCheck = option.EnableHealthCheck;

            EnableReverseProxy = option.EnableReverseProxy;
            ReverseProxyMap = option.ReverseProxyMap;

            ReceiptService = option.ReceiptService;
            ReceiptApi = option.ReceiptApi;
            EnableReceipt = option.EnableReceipt && ReceiptService.IsPresent() && ReceiptApi.IsPresent();

            JwtIssue = option.JwtIssue;
            JwtAppSecretByte = option.JwtAppSecret?.ToUtf8Bytes();
            EnableJwtToken = option.EnableJwtToken && JwtIssue.IsPresent() && JwtAppSecretByte != null;
        }
        #endregion
    }
}
