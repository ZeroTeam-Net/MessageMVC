using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.WechatEx
{

    /// <summary>
    /// 令牌校验配置
    /// </summary>
    public class WechatOption
    {
        /// <summary>
        /// 微信AppID
        /// </summary>
        public string AppID { get; set; }

        /// <summary>
        /// 微信AppSecret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 令牌
        /// </summary>
        public string AccessToken;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Last { get; set; }

        /// <summary>
        /// 唯一实例
        /// </summary>
        public readonly static WechatOption Instance = new WechatOption();

        /// <summary>
        /// 模板预设
        /// </summary>
        public Dictionary<string, WechatTemplate> Templates { get; set; } = new Dictionary<string, WechatTemplate>();

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        static WechatOption()
        {
            ConfigurationHelper.RegistOnChange<WechatOption>("Wechat", Instance.LoadConfig, true);
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        void LoadConfig(WechatOption option)
        {
            AppID = option.AppID;
            AppSecret = option.AppSecret;
            if (AppID.IsMissing() || AppSecret.IsMissing())
            {
                ScopeRuner.ScopeLogger.Warning("微信接口配置中，AppID或AppSecret为空，后续接口调用将不正常");
            }
            if (option.Templates != null)
                Templates = option.Templates;
        }

        #endregion

    }
}