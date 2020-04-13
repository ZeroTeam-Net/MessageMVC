using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.ZeroApis
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
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Argument<T> : IApiArgument
    {
        /// <summary>
        ///     ��ֵ
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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