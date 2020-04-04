using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API返回基类
    /// </summary>
    public interface IApiResult : IOperatorStatus
    {
    }

    /// <summary>
    ///     API返回基类
    /// </summary>
    public interface IApiResult<out TData> : IApiResult
    {
        /// <summary>
        ///     返回值
        /// </summary>
        [JsonProperty("data")]
        TData ResultData { get; }
    }
}