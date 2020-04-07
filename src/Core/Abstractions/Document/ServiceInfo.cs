using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    /// 站点信息
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ServiceInfo : ServiceDocument
    {
        /// <summary>
        /// 消息接收对象构造器
        /// </summary>
        public Func<string, IMessageReceiver> NetBuilder { get; set; }
    }
}