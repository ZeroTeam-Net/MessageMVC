using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ServiceDocument : DocumentItem
    {
        /// <summary>
        /// 序列化类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SerializeType Serialize { get; set; }

        /// <summary>
        ///     是否本地
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLocal { get; set; } = true;

        /// <summary>
        ///     Api方法
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ApiDocument> Aips { get; set; }
            = new Dictionary<string, ApiDocument>(StringComparer.OrdinalIgnoreCase);
    }
}