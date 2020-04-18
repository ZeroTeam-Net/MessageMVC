using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     Api结构的信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TypeDocument : DocumentItem
    {
        /// <summary>
        ///     类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ClassName { get; set; }

        /// <summary>
        ///     枚举
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEnum { get; set; }

        /// <summary>
        ///     Json名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string JsonName { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ObjectType ObjectType { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TypeName { get; set; }

        /// <summary>
        ///     能否为空
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool CanNull { get; set; }

        /// <summary>
        ///     正则校验(文本)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Regex { get; set; }

        /// <summary>
        ///     最小(包含的数值或文本长度)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Min { get; set; }

        /// <summary>
        ///     最大(包含的数值或文本长度)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Max { get; set; }

        /// <summary>
        ///     字段
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, TypeDocument> Fields { get; set; } = new Dictionary<string, TypeDocument>();
    }
}