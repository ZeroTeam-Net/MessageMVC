using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// 跟踪消息
    /// </summary>
    public class TraceLinkMessage
    {
        /// <summary>
        /// 跟踪
        /// </summary>
        public TraceInfo Trace { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public MessageItem Message { get; set; }

        /// <summary>
        /// 本地跟踪
        /// </summary>
        public TraceStep Monitor { get; set; }
    }
}

/*
 * 
        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("argumentType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ArgumentType ArgumentType { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("requestType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        [JsonProperty("ip", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        /// <summary>
        /// 请求端口号
        /// </summary>
        [JsonProperty("port", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Port { get; set; }
        /// <summary>
        ///     当前用户登录到哪个系统（预先定义的系统标识）
        /// </summary>
        [JsonProperty("app", NullValueHandling = NullValueHandling.Ignore)]
        public string App { get; set; }

        /// <summary>
        /// HTTP的UserAgent
        /// </summary>
        [JsonProperty("userAgent", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string UserAgent { get; set; }


        /// <summary>
        ///     登录设备的操作系统
        /// </summary>
        [JsonProperty("os", NullValueHandling = NullValueHandling.Ignore)]
        public string Os { get; set; }
 
*/
