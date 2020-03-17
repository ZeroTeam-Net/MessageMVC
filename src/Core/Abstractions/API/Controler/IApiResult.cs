using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API返回基类
    /// </summary>
    public interface IApiResult
    {
        /// <summary>
        ///     成功或失败标记
        /// </summary>
        [JsonProperty("success")]
        bool Success { get; set; }

        /// <summary>
        ///     错误码（系统定义）
        /// </summary>
        [JsonProperty("code")]
        int Code { get; set; }

        /// <summary>
        ///  信息
        /// </summary>
        [JsonProperty("message")]
        string Message { get; set; }
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