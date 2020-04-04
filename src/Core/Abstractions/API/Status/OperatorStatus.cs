using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API状态返回接口实现
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperatorStatus : IOperatorStatus
    {
        /// <summary>
        ///     成功或失败标记
        /// </summary>
        /// <example>true</example>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        ///     错误码
        /// </summary>
        /// <remarks>
        ///     参见 ErrorCode 说明
        /// </remarks>
        /// <example>-1</example>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        ///     提示信息
        /// </summary>
        /// <remarks>
        ///  说明错误的原因
        /// </remarks>
        /// <example>你的数据不正确</example>
        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        ///     内部提示信息
        /// </summary>
        public string InnerMessage { get; set; }

    }
}