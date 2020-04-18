using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
{
    /// <summary>
    ///     API接口跟踪
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OperatorTrace : IOperatorTrace
    {
        /// <summary>
        ///     默认构造
        /// </summary>
        public OperatorTrace()
        {
            if (ContractOption.Instance.EnableResultTrace)
            {
                Point = ContractOption.Instance.TraceMachine
                    ? $"{GlobalContext.CurrentNoLazy?.Trace?.LocalApp}|{GlobalContext.CurrentNoLazy?.Trace?.LocalMachine}"
                    : GlobalContext.CurrentNoLazy?.Trace?.LocalApp;
                RequestId = GlobalContext.CurrentNoLazy?.Trace?.TraceId;
            }
        }

        /// <summary>
        ///     API请求标识
        /// </summary>
        /// <example>AxV6389FC</example>
        [DataMember(Name = "requestId"), JsonPropertyName("requestId"), JsonProperty("requestId", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string RequestId { get; set; }

        /// <summary>
        ///     错误点
        /// </summary>
        /// <remarks>
        /// 系统在哪一个节点发生错误的标识
        /// </remarks>
        /// <example>http-gateway</example>
        [DataMember(Name = "point"), JsonPropertyName("point"), JsonProperty("point", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Point { get; set; }

        /// <summary>
        ///     指导码
        /// </summary>
        /// <remarks>
        /// 内部使用:指示下一步应如何处理的代码
        /// </remarks>
        /// <example>retry</example>
        [DataMember(Name = "guide"), JsonPropertyName("guide"), JsonProperty("guide", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Guide { get; set; }

        /// <summary>
        ///     错误说明
        /// </summary>
        /// <remarks>
        /// 内部使用:详细说明错误内容
        /// </remarks>
        /// <example>系统未就绪</example>
        [DataMember(Name = "desc"), JsonPropertyName("desc"), JsonProperty("desc", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Describe { get; set; }

    }
}