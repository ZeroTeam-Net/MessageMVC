using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API状态返回接口实现
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperatorStatus : IOperatorStatus
    {
        /// <summary>
        ///     默认构造
        /// </summary>
        public OperatorStatus()
        {
            Point = GlobalContext.ServiceName;
        }

        /// <summary>
        ///     默认构造
        /// </summary>
        public OperatorStatus(int code, string messgae)
        {
            Code = code;
            Message = messgae;
        }

        /// <summary>
        ///     错误码
        /// </summary>
        /// <remarks>
        ///     参见 ErrorCode 说明
        /// </remarks>
        /// <example>-1</example>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public int Code { get; set; }

        /// <summary>
        ///     提示信息
        /// </summary>
        /// <remarks>
        ///  说明错误的原因
        /// </remarks>
        /// <example>你的数据不正确</example>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        ///     内部提示信息
        /// </summary>
        public string InnerMessage { get; set; }

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

        /// <summary>
        ///     错误点
        /// </summary>
        /// <remarks>
        /// 系统在哪一个节点发生错误的标识
        /// </remarks>
        /// <example>http-gateway</example>
        [JsonProperty("point", NullValueHandling = NullValueHandling.Ignore)]
        public string Point { get; set; }

    }
}