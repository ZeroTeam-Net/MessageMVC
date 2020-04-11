using Newtonsoft.Json;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API返回基类
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiResult : OperatorStatus, IApiResult
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ApiResult()
        {
            if (GlobalContext.EnableLinkTrace)
            {
                Trace = new OperatorTrace();
            }

            Success = true;
        }

        /// <summary>
        ///     执行跟踪
        /// </summary>
        [DataMember, JsonProperty("trace", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IOperatorTrace Trace { get; set; }

    }


    /// <summary>
    ///     API返回数据泛型类
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiResult<TData> : ApiResult, IApiResult<TData>
    {
        /// <summary>
        ///     返回值
        /// </summary>
        [DataMember, JsonProperty("data")]
        public TData ResultData { get; set; }

    }
}