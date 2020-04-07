﻿using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     文档节点
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DocumentItem : AnnotationsConfig
    {

        [JsonProperty("example", NullValueHandling = NullValueHandling.Ignore)]
        private string _example;


        [JsonProperty("seealso", NullValueHandling = NullValueHandling.Ignore)]
        private string _seealso;


        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        private string _value;

        /// <summary>
        ///     参见
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore]
        public string Seealso
        {
            get => _seealso;
            set => _seealso = value;
        }

        /// <summary>
        ///     示例
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore]
        public string Example
        {
            get => _example;
            set => _example = value;
        }

        /// <summary>
        ///     值描述
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore]
        public string Value
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        ///     复制
        /// </summary>
        /// <param name="document"></param>
        public void Copy(DocumentItem document)
        {
            if (document == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(document.Caption))
            {
                Caption = document.Caption;
            }

            if (!string.IsNullOrWhiteSpace(document.Description))
            {
                Description = document.Description;
            }

            if (!string.IsNullOrWhiteSpace(document.Seealso))
            {
                Seealso = document.Seealso;
            }

            if (!string.IsNullOrWhiteSpace(document.Example))
            {
                Example = document.Example;
            }

            if (!string.IsNullOrWhiteSpace(document.Value))
            {
                Value = document.Value;
            }
        }
    }
}