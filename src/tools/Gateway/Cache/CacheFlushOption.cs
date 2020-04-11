using System.Collections.Generic;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     缓存触发刷新设置
    /// </summary>
    public class CacheFlushOption
    {
        /// <summary>
        ///     触发更新的API名称
        /// </summary>
        public string TriggerApi { get; set; }

        /// <summary>
        ///     需要缓存的API名称
        /// </summary>
        public string CacheApi { get; set; }

        /// <summary>
        ///  字段映射
        /// </summary>
        public Dictionary<string, string> ArgumentMap { get; set; }

    }
}