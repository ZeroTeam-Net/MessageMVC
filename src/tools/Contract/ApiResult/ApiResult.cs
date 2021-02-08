using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
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
            if (ContractOption.Instance.EnableResultTrace)
                Trace = new OperatorTrace();
            Success = true;
            RequestId = GlobalContext.CurrentNoLazy?.Message.RequestId;
        }

        /// <summary>
        ///     执行跟踪
        /// </summary>
        IOperatorTrace IApiResult.Trace
        {
            get => Trace;
            set
            {
                if (value == null)
                {
                    Trace = null;
                    return;
                }
                if (value is OperatorTrace trace)
                {
                    Trace = trace;
                    return;
                }
                Trace = new OperatorTrace
                {
                    RequestId = value.RequestId,
                    Point = value.Point,
                    Guide = value.Guide,
                    Describe = value.Describe,
                };
            }
        }

        /// <summary>
        ///     执行跟踪
        /// </summary>
        [DataMember(Name = "trace"), JsonPropertyName("trace"), JsonProperty("trace", NullValueHandling = NullValueHandling.Ignore)]
        public OperatorTrace Trace { get; set; }

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
        [DataMember(Name = "data"), JsonPropertyName("data"), JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public TData ResultData { get; set; }

    }


    /// <summary>
    ///     API返回数据泛型类
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DependencyApiResult<TData> : ApiResult, IApiResult<TData>
    {
        /// <summary>
        ///     返回值
        /// </summary>
        [DataMember(Name = "data"), JsonPropertyName("data"), JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public TData ResultData { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public DependencyApiResult()
        {
            ResultData = DependencyHelper.GetService<TData>();
        }

    }
}