using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API接口跟踪
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperatorTrace
    {
        /// <summary>
        ///     默认构造
        /// </summary>
        public OperatorTrace()
        {
            if (GlobalContext.CurrentNoLazy?.Option["EnableLinkTrace"] == "true")
            {
                Point = $"{GlobalContext.CurrentNoLazy?.Trace?.LocalMachine}|{GlobalContext.CurrentNoLazy?.Trace?.LocalApp}";
                Timestamp = GlobalContext.CurrentNoLazy?.Trace?.LocalTimestamp;
                RequestId = GlobalContext.CurrentNoLazy?.Trace?.TraceId;
            }
        }

        /// <summary>
        ///     API请求标识
        /// </summary>
        /// <example>AxV6389FC</example>
        [JsonProperty("requestId", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string RequestId { get; set; }

        /// <summary>
        /// 生产时间戳,UNIX时间戳,自1970起秒数
        /// </summary>
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public int? Timestamp { get; set; }

        /// <summary>
        ///     错误点
        /// </summary>
        /// <remarks>
        /// 系统在哪一个节点发生错误的标识
        /// </remarks>
        /// <example>http-gateway</example>
        [JsonProperty("point", NullValueHandling = NullValueHandling.Ignore)]
        public string Point { get; set; }

        /// <summary>
        ///     指导码
        /// </summary>
        /// <remarks>
        /// 内部使用:指示下一步应如何处理的代码
        /// </remarks>
        /// <example>retry</example>
        [JsonProperty("guide", NullValueHandling = NullValueHandling.Ignore)]
        public string Guide { get; set; }

        /// <summary>
        ///     错误说明
        /// </summary>
        /// <remarks>
        /// 内部使用:详细说明错误内容
        /// </remarks>
        /// <example>系统未就绪</example>
        [JsonProperty("describe", NullValueHandling = NullValueHandling.Ignore)]
        public string Describe { get; set; }

    }
}