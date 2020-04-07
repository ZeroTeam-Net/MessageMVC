using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     Api结构的信息
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TypeDocument : DocumentItem
    {
        /// <summary>
        ///     类型
        /// </summary>

        [JsonProperty("class", NullValueHandling = NullValueHandling.Ignore)]
        public string ClassName;

        /// <summary>
        ///     字段
        /// </summary>

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, TypeDocument> fields;

        /// <summary>
        ///     枚举
        /// </summary>

        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEnum;

        /// <summary>
        ///     类型
        /// </summary>

        [JsonProperty("jsonName", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonName;

        /// <summary>
        ///     类型
        /// </summary>

        [JsonProperty("object", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectType ObjectType;

        /// <summary>
        ///     类型
        /// </summary>

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string TypeName;

        /// <summary>
        ///     能否为空
        /// </summary>

        [JsonProperty("canNull", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanNull { get; set; }

        /// <summary>
        ///     正则校验(文本)
        /// </summary>

        [JsonProperty("regex", NullValueHandling = NullValueHandling.Ignore)]
        public string Regex { get; set; }

        /// <summary>
        ///     最小(包含的数值或文本长度)
        /// </summary>

        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public string Min { get; set; }

        /// <summary>
        ///     最大(包含的数值或文本长度)
        /// </summary>

        [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public string Max { get; set; }

        /// <summary>
        ///     字段
        /// </summary>
        public Dictionary<string, TypeDocument> Fields => fields ?? (fields = new Dictionary<string, TypeDocument>());
    }
}