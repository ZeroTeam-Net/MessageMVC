using Agebull.Common.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     缓存设置
    /// </summary>
    public class ApiCacheOption
    {
        /// <summary>
        ///     API名称
        /// </summary>
        public string Api { get; set; }


        /// <summary>
        ///     缓存的参数依据
        /// </summary>
        public List<string> Argument { get; set; }

        /// <summary>
        ///     是否包含用户身份
        /// </summary>
        public bool Bear { get; set; }

        /// <summary>
        ///     缓存更新的秒数
        /// </summary>
        public int FlushSecond { get; set; }

        /// <summary>
        ///     缓存时仅使用名称（否则包含查询字符串）
        /// </summary>
        public bool OnlyName { get; set; }

        /// <summary>
        ///     发生网络错误时缓存
        /// </summary>
        public bool ByNetError { get; set; }

        /// <summary>
        ///     缓存特征
        /// </summary>
        public CacheFeature Feature { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        internal void Initialize()
        {
            //默认5分钟
            if (FlushSecond <= 0)
            {
                FlushSecond = 300;
            }
            else if (FlushSecond > 3600)
            {
                FlushSecond = 3600;
            }

            if (Argument != null && Argument.Count > 0)
            {
                Feature = CacheFeature.Keys;
                return;
            }
            if (Bear)
            {
                Feature |= CacheFeature.Bear;
            }

            if (!OnlyName)
            {
                Feature |= CacheFeature.QueryString;
            }

            if (ByNetError)
            {
                Feature |= CacheFeature.NetError;
            }
        }
    }
}