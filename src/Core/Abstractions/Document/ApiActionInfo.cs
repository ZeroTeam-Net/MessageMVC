using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.Messages;
using ApiFunc = System.Func<ZeroTeam.MessageMVC.Messages.IInlineMessage, ZeroTeam.MessageMVC.Messages.ISerializeProxy, object, object>;


namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiActionInfo : ApiDocument
    {
        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumentType;

        /// <summary>是否有调用参数</summary>
        public Type ResultType;

        /// <summary>有参方法</summary>
        public ApiFunc Action;

    }
}