using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MicroZero.Http.Gateway
{

    /// <summary>
    ///     缓存设置
    /// </summary>
    public class CacheOption
    {
        #region 配置自动更新


        /// <summary>
        ///     缓存数据
        /// </summary>
        internal static readonly ConcurrentDictionary<string, CacheData> Cache = new ConcurrentDictionary<string, CacheData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     缓存配置
        /// </summary>
        internal static Dictionary<string, ApiCacheOption> CacheMap = new Dictionary<string, ApiCacheOption>(StringComparer.OrdinalIgnoreCase);

        static CacheOption()
        {
            ConfigurationManager.RegistOnChange(LoadOption, true);

        }
        static void LoadOption()
        {
            Cache.Clear();
            CacheMap.Clear();
            var apis = ConfigurationManager.Option<ApiCacheOption[]>("Gateway:Chache");
            if (apis == null || apis.Length == 0)
                return;

            foreach (var setting in apis)
            {
                setting.Initialize();
                if (!CacheMap.ContainsKey(setting.Api))
                {
                    CacheMap.Add(setting.Api, setting);
                }
                else
                {
                    CacheMap[setting.Api] = setting;
                }
            }
        }
        #endregion

    }
}