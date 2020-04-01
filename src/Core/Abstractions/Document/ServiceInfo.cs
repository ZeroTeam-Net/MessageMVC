using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiDocuments
{
    /// <summary>
    /// 站点信息
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceInfo : ServiceDocument
    {
        /// <summary>
        /// 网络传输对象构造器
        /// </summary>
        public Func<string, INetTransfer> NetBuilder { get; set; }
    }
}