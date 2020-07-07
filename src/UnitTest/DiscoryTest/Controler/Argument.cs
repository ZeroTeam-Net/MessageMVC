using Newtonsoft.Json;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    /// <summary>
    ///     请求参数
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TestArgument : IApiArgument
    {
        /// <summary>
        ///     文本内容
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        /// <summary>
        ///     数据校验
        /// </summary>
        /// <param name="message">status</param>
        /// <returns>成功则返回真</returns>
        public bool Validate(out IOperatorStatus status)
        {
            if (Value == null)
            {
                status = ApiResultHelper.State(OperatorStatusCode.ArgumentError, "参数不能为空");
                return false;
            }
            status = null;
            return true;
        }
    }

}
