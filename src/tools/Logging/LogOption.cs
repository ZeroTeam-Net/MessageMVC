using Agebull.Common.Configuration;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC;

namespace Agebull.Common.Logging
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    public class LogOption : IZeroOption
    {
        
        /// <summary>
        ///     独立的等级
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        ///     启用
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

        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly LogOption Instance = new LogOption();

        const string sectionName = "MessageMVC:MessageLogger";

        const string optionName = "HttpClient配置";

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
        bool IZeroOption.IsDynamic => true;

        void IZeroOption.Load(bool first)
        {
            LogOption option = ConfigurationHelper.Get<LogOption>(sectionName);

            if (first)
            {
                if (option == null)
                {
                    Enable = true;
                    Level = LogLevel.Information;
                    Service = "log";
                    LogApi = "text";
                    MonitorApi = "monitor";
                }
                ConfigurationHelper.RegistOnChange<LogOption>("MessageMVC:Tools", Update, false);
            }
            if (option != null)
            {
                Update(option);
            }
        }

        void Update(LogOption option)
        {
            Level = option.Level;
            Enable = option.Enable;
            Service = option.Service ?? "log";
            LogApi = option.LogApi ?? "text";
            MonitorApi = option.MonitorApi ?? "monitor";
        }
        #endregion
    }
}
