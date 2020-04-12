using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API返回基类
    /// </summary>
    public interface IApiResult : IOperatorStatus
    {
        /// <summary>
        ///跟踪对象
        /// </summary>
        IOperatorTrace Trace { get; set; }
    }

    /// <summary>
    ///     API返回基类
    /// </summary>
    public interface IApiResult<TData> : IApiResult
    {
        /// <summary>
        ///     返回值
        /// </summary>
        [JsonProperty("data")]
        TData ResultData { get; set; }
    }
}