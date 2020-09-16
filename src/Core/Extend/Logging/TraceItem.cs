using Newtonsoft.Json;

namespace Agebull.Common.Logging
{
    /// <summary>
    ///     跟踪信息
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TraceItem
    {
        /// <summary>
        ///     文本
        /// </summary>
        public string Message;
    }
}