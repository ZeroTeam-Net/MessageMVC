using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.ApiDocuments
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [DataContract]
        [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ServiceDocument : DocumentItem
    {
        /// <summary>
        ///     Api方法
        /// </summary>
        [DataMember]
        [JsonProperty("aips", NullValueHandling = NullValueHandling.Ignore)]
        private Dictionary<string, ApiDocument> aips;

        /// <summary>
        ///     是否本地
        /// </summary>
        public bool IsLocal { get; set; } = true;

        /// <summary>
        ///     Api方法
        /// </summary>
        public Dictionary<string, ApiDocument> Aips =>
            aips ?? (aips = new Dictionary<string, ApiDocument>(StringComparer.OrdinalIgnoreCase));
    }
}