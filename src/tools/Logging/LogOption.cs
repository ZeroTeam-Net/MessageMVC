using Agebull.Common.Configuration;

namespace Agebull.Common.Logging
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    public class LogOption
    {
        /// <summary>
        ///     启用埋点
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        ///     日志服务
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        ///     日志API
        /// </summary>
        public string LogApi { get; set; }

        /// <summary>
        ///     监控API
        /// </summary>
        public string MonitorApi { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly LogOption Instance = new LogOption();

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        public static void LoadOption()
        {
            LogOption option = ConfigurationHelper.Get<LogOption>("Logging:Message");

            if (option == null)
            {
                Instance.Enable = true;
                Instance.Service = "log";
                Instance.LogApi = "text";
                Instance.MonitorApi = "monitor";
            }
            else
            {
                Instance.Enable = option.Enable;
                Instance.Service = option.Service ?? "log";
                Instance.LogApi = option.LogApi ?? "text";
                Instance.MonitorApi = option.MonitorApi ?? "monitor";
            }
        }
    }
}
