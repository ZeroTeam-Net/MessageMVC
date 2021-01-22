using Agebull.Common.Configuration;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    public class ToolsOption
    {
        /// <summary>
        ///     启用调用链跟踪
        /// </summary>
        public bool EnableLinkTrace => ZeroAppOption.Instance.TraceInfo.HasFlag(TraceInfoType.LinkTrace);

        /// <summary>
        ///     启用反向代理
        /// </summary>
        public bool EnableReverseProxy { get; set; }

        /// <summary>
        ///     反向代理服务名称映射
        /// </summary>
        public Dictionary<string,string> ReverseProxyMap { get; set; }

        /// <summary>
        ///     启用Monitor模式日志记录
        /// </summary>
        public bool EnableMonitorLog { get; set; }

        /// <summary>
        ///     启用第三方回执
        /// </summary>
        public bool EnableReceipt { get; set; }

        /// <summary>
        ///     启用埋点
        /// </summary>
        public bool EnableMarkPoint { get; set; }

        /// <summary>
        ///     埋点服务名称
        /// </summary>
        public string MarkPointName { get; set; }

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

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ToolsOption Instance = new ToolsOption();

        static ToolsOption()
        {
            ConfigurationHelper.RegistOnChange<ToolsOption>("MessageMVC:Tools", Instance.Update, true);
        }

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        private void Update(ToolsOption option)
        {
            EnableMonitorLog = option.EnableMonitorLog;
            EnableMessageReConsumer = option.EnableMessageReConsumer;

            EnableReverseProxy = option.EnableReverseProxy;
            ReverseProxyMap = option.ReverseProxyMap;

            ReceiptService = option.ReceiptService;
            ReceiptApi = option.ReceiptApi;
            EnableReceipt = option.EnableReceipt && !string.IsNullOrWhiteSpace(ReceiptService) && !string.IsNullOrWhiteSpace(ReceiptApi);

            JwtIssue = option.JwtIssue;
            JwtAppSecretByte = option.JwtAppSecret?.ToUtf8Bytes();
            EnableJwtToken = option.EnableJwtToken && !string.IsNullOrWhiteSpace(JwtIssue) && JwtAppSecretByte != null;

            MarkPointName = option.MarkPointName;
            EnableMarkPoint = option.EnableMarkPoint && !string.IsNullOrWhiteSpace(MarkPointName);
        }
    }
}
