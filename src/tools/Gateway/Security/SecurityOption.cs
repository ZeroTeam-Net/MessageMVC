using Agebull.Common.Configuration;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     安全相关的配置
    /// </summary>
    public class SecurityOption
    {

        /// <summary>
        ///     检查API访问权限
        /// </summary>
        public bool CheckApiAccess { get; set; }

        /// <summary>
        ///     需要检查的Api
        /// </summary>
        public Dictionary<string, ApiCheckInfo> CheckApis { get; set; }

        /// <summary>
        ///     启用验签
        /// </summary>
        public bool EnableSign { get; set; }

        /// <summary>
        ///     是否检查Auth头
        /// </summary>
        public bool EnableAuth { get; set; }

        /// <summary>
        ///     鉴权服务名称
        /// </summary>
        public string AuthService { get; set; }

        /// <summary>
        ///     令牌检查API
        /// </summary>
        public string TokenCheckApi { get; set; }

        /// <summary>
        ///     黑名单令牌
        /// </summary>
        public HashSet<string> DenyTokens { get; set; }

        /// <summary>
        ///     禁止的Http头信息
        /// </summary>
        public Dictionary<string, DenyHttpHeaderItem> DenyHttpHeaders { get; set; }


        #region 配置自动更新

        static SecurityOption()
        {
            ConfigurationManager.RegistOnChange(LoadOption, true);

        }
        static void LoadOption()
        {
            var option = ConfigurationManager.Option<SecurityOption>("Gateway:Security");
            if (option == null)
                return;
            Instance.CheckApiAccess = option.CheckApiAccess;
            Instance.EnableSign = option.EnableSign;
            Instance.EnableAuth = option.EnableAuth;
            Instance.AuthService = option.AuthService;
            Instance.TokenCheckApi = option.TokenCheckApi;

            Instance.DenyTokens.Clear();
            if (option.DenyTokens != null)
            {
                foreach (var item in option.DenyTokens)
                    Instance.DenyTokens.Add(item);
            }
            Instance.CheckApis.Clear();
            if (option.CheckApis != null)
            {
                foreach (var item in option.CheckApis)
                    Instance.CheckApis.TryAdd(item.Key, item.Value);
            }
            Instance.DenyHttpHeaders.Clear();
            if (option.DenyHttpHeaders != null)
            {
                foreach (var item in option.DenyHttpHeaders)
                    Instance.DenyHttpHeaders.TryAdd(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 配置
        /// </summary>
        public static readonly SecurityOption Instance = new SecurityOption
        {
            DenyTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            CheckApis = new Dictionary<string, ApiCheckInfo>(StringComparer.OrdinalIgnoreCase),
            DenyHttpHeaders = new Dictionary<string, DenyHttpHeaderItem>(StringComparer.OrdinalIgnoreCase),
        };

        #endregion
    }
}