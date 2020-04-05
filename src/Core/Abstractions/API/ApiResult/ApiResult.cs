using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API返回基类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiResult : OperatorStatus, IApiResult
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ApiResult()
        {
            if (GlobalContext.EnableLinkTrace)
                Trace = new OperatorTrace
                {
                    Point = $"{GlobalContext.CurrentNoLazy?.Trace?.LocalMachine}|{GlobalContext.CurrentNoLazy?.Trace?.LocalApp}",
                    Timestamp = GlobalContext.CurrentNoLazy?.Trace?.LocalTimestamp,
                    RequestId = GlobalContext.CurrentNoLazy?.Trace?.TraceId,
                };
            Success = true;
        }

        /// <summary>
        ///     执行跟踪
        /// </summary>
        [JsonProperty("trace", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public OperatorTrace Trace { get; set; }

    }


    /// <summary>
    ///     API返回数据泛型类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiResult<TData> : ApiResult, IApiResult<TData>
    {
        /// <summary>
        ///     返回值
        /// </summary>
        public TData Data => ResultData;

        /// <summary>
        ///     返回值
        /// </summary>
        [JsonProperty("data")]
        public TData ResultData { get; set; }

    }
}