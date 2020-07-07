using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public string TypeName { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ClassName { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsBaseType { get; set; }

        /// <summary>
        ///     枚举
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEnum { get; set; }

        /// <summary>
        ///     数组
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsArray { get; set; }

        /// <summary>
        ///     Json名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string JsonName { get; set; }

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

        /// <summary>
        ///     复制
        /// </summary>
        /// <param name="document"></param>
        public override void Copy(DocumentItem document)
        {
            base.Copy(document);
            if (!(document is TypeDocument type))
                return;
            IsBaseType = type.IsBaseType;
            IsEnum = type.IsEnum;
            IsArray = type.IsArray;
            ClassName = type.ClassName;
            JsonName = type.JsonName;
            TypeName = type.TypeName;
            CanNull = type.CanNull;
            Regex = type.Regex;
            Min = type.Min;
            Max = type.Max;
            Fields = type.Fields;
        }

        /// <summary>
        /// 文档说明
        /// </summary>
        public string DocDesc => (Caption ?? Description ?? "-").Replace('|', '/').Replace('\n', ' ').Replace('\r', ' ');

        /// <summary>
        /// 文档名称
        /// </summary>
        public string DocName => JsonName ?? Name ?? "-";

        /// <summary>
        /// 文档名称
        /// </summary>
        public string DocExample
        {
            get
            {
                var code = new StringBuilder();
                if (IsArray)
                {
                    code.Append('[');
                }
                if (Example != null)
                {
                    code.Append(Example);
                }
                else if (IsBaseType)
                {
                    switch (TypeName)
                    {
                        case "string":
                            code.Append("\"示例文本\"");
                            break;
                        case "bool":
                            code.Append("true");
                            break;
                        case "DateTime":
                            code.Append("\"2020-1-23T00:00:00.8888\"");
                            break;
                        case "Guid":
                            code.Append($"\"{Guid.NewGuid()}\"");
                            break;
                        case "int":
                        case "long":
                            code.Append("1");
                            break;
                        default:
                            code.Append("");
                            break;
                    }
                }
                if (IsArray)
                {
                    code.Append(']');
                }
                return code.ToString();
            }
        }

    }
}