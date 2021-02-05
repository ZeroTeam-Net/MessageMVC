using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.PageInfoAutoSave
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    public class ToolsOption : IZeroOption
    {
        /// <summary>
        ///     启用页面信息记录
        /// </summary>
        public bool EnablePageInfo { get; set; }

        /// <summary>
        ///     页面信息服务名称
        /// </summary>
        public string PageInfoService { get; set; }

        /// <summary>
        ///     页面信息接口方法
        /// </summary>
        public string PageInfoApi { get; set; }

        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ToolsOption Instance = new ToolsOption();

        const string sectionName = "MessageMVC:Tools";

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
            if (first)
                ConfigurationHelper.RegistOnChange<ToolsOption>("MessageMVC:Tools", Update, true);
        }

        void Update(ToolsOption option)
        {
            PageInfoService = option.PageInfoService;
            PageInfoApi = option.PageInfoApi;
            EnablePageInfo = option.EnablePageInfo && !string.IsNullOrWhiteSpace(PageInfoService) && !string.IsNullOrWhiteSpace(PageInfoApi);
        }
        #endregion
    }
}