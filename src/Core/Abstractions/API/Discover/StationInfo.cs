using System;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;
using Newtonsoft.Json;
using ZeroTeam.MessageMVC.ApiDocuments;

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
        public Func<string,INetTransport> NetBuilder { get; set; }
    }
}