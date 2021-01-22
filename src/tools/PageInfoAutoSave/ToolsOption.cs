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
            PageInfoService = option.PageInfoService;
            PageInfoApi = option.PageInfoApi;
            EnablePageInfo = option.EnablePageInfo && !string.IsNullOrWhiteSpace(PageInfoService) && !string.IsNullOrWhiteSpace(PageInfoApi);
        }
    }
}
