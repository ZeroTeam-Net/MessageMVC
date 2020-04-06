using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    public class ToolsOption
    {

        /// <summary>
        ///     启用调用链跟踪(使用IZeroContext全局上下文)
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

        public string ReceiptService { get; set; } = "TrdReceipt";


        /// <summary>
        ///     回执接口方法
        /// </summary>

        public string ReceiptApi { get; set; } = "receipt/v1/save";


        /// <summary>
        /// 实例
        /// </summary>
        public readonly static ToolsOption Instance = new ToolsOption();

        static ToolsOption()
        {
            ConfigurationManager.RegistOnChange(Instance.Update, true);
        }

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        private void Update()
        {
            ToolsOption option = ConfigurationManager.Get<ToolsOption>("ZeroApp.Tools");

            ReceiptService = option.ReceiptService;
            ReceiptApi = option.ReceiptApi;
            EnableLinkTrace = option.EnableLinkTrace;
            EnableMonitorLog = option.EnableMonitorLog;
            EnableMessageReConsumer = option.EnableMessageReConsumer;
            EnableMarkPoint = option.EnableMarkPoint;
            MarkPointName = option.MarkPointName;
        }
    }
}
