using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     操作状态
    /// </summary>
    public interface IOperatorStatus
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
}