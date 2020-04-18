using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     文档节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DocumentItem : AnnotationsConfig
    {
        /// <summary>
        ///     参见
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Seealso { get; set; }

        /// <summary>
        ///     示例
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore]
        public string Example { get; set; }

        /// <summary>
        ///     值描述
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

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
                Example = document.Example.Trim();
            }

            if (!string.IsNullOrWhiteSpace(document.Value))
            {
                Value = document.Value;
            }
        }
    }
}