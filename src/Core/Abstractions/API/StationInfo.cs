using System;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;
using Newtonsoft.Json;
using Agebull.MicroZero.ApiDocuments;

namespace ZeroTeam.MessageMVC.ApiDocuments
{
    /// <summary>
    /// 站点信息
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceInfo : StationDocument
    {
        /// <summary>
        /// 网络传输对象构造器
        /// </summary>
        public Func<string,INetTransport> NetBuilder { get; set; }
    }
}