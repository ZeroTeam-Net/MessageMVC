using Agebull.Common.Configuration;
using System;

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

        public bool EnableLinkTrace { get; set; }

        /// <summary>
        ///     启用Monitor模式日志记录
        /// </summary>

        public bool EnableMonitorLog { get; set; }

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
        public static readonly ToolsOption Instance = new ToolsOption
        {
            ReceiptService = "TrdReceipt",
            ReceiptApi = "receipt/v1/save",
            MarkPointName = "MarkPoint"
        };

        static ToolsOption()
        {
            ConfigurationHelper.RegistOnChange<ToolsOption>("MessageMVC:Tools", Instance.Update, true);
        }

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        private void Update(ToolsOption option)
        {
            EnableLinkTrace = option.EnableLinkTrace;
            EnableMonitorLog = option.EnableMonitorLog;
            EnableMessageReConsumer = option.EnableMessageReConsumer;
            EnableMarkPoint = option.EnableMarkPoint;
            if (!string.IsNullOrWhiteSpace(option.ReceiptService))
                ReceiptService = option.ReceiptService;
            if (!string.IsNullOrWhiteSpace(option.ReceiptApi))
                ReceiptApi = option.ReceiptApi;
            if (!string.IsNullOrWhiteSpace(option.ReceiptApi))
                ReceiptApi = option.ReceiptApi;

            JwtAppSecretByte = option.JwtAppSecret.ToUtf8Bytes();
            JwtIssue = option.JwtIssue;
        }
    }
}
