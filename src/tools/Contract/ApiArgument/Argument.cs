using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
{
    /// <summary>
    ///     请求参数
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Argument : IApiArgument
    {
        /// <summary>
        ///     文本内容
        /// </summary>
        [DataMember(Name = "value"), JsonPropertyName("value"), JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        /// <summary>
        ///     数据校验
        /// </summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public bool Validate(out string message)
        {
            message = null;
            return true;
        }
    }

    /// <summary>
    ///     请求参数
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Argument<T> : IApiArgument
    {
        /// <summary>
        ///     数值
        /// </summary>
        [DataMember(Name = "value"), JsonPropertyName("value"), JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public T Value { get; set; }

        /// <summary>
        ///     数据校验
        /// </summary>
        /// <param name="message">返回的消息</param>
        /// <returns>成功则返回真</returns>
        public bool Validate(out string message)
        {
            message = null;
            return true;
        }
    }
}