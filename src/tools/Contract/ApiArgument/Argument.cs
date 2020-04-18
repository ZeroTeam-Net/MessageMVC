using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
{
    /// <summary>
    ///     �������
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Argument : IApiArgument
    {
        /// <summary>
        ///     �ı�����
        /// </summary>
        [DataMember(Name = "value"), JsonPropertyName("value"), JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        /// <summary>
        ///     ����У��
        /// </summary>
        /// <param name="message">���ص���Ϣ</param>
        /// <returns>�ɹ��򷵻���</returns>
        public bool Validate(out string message)
        {
            message = null;
            return true;
        }
    }

    /// <summary>
    ///     �������
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Argument<T> : IApiArgument
    {
        /// <summary>
        ///     ��ֵ
        /// </summary>
        [DataMember(Name = "value"), JsonPropertyName("value"), JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public T Value { get; set; }

        /// <summary>
        ///     ����У��
        /// </summary>
        /// <param name="message">���ص���Ϣ</param>
        /// <returns>�ɹ��򷵻���</returns>
        public bool Validate(out string message)
        {
            message = null;
            return true;
        }
    }
}