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
    [DataContract]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiActionInfo : ApiDocument
    {
        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumentType;


        /// <summary>
        ///     参数类型
        /// </summary>
        public Dictionary<string,Type> Arguments;

        /// <summary>是否有调用参数</summary>
        public Type ResultType;

        /// <summary>
        ///     所在控制器类型
        /// </summary>
        public string Controller;

        /// <summary>
        ///     是否有调用参数
        /// </summary>
        public bool HaseArgument;

        /// <summary>
        ///     是否异步任务
        /// </summary>
        public bool IsAsync;

        /// <summary>有参方法</summary>
        public ApiFunc Action;


        /// <summary>
        /// 反序列化类型
        /// </summary>
        public SerializeType ArgumentSerializeType;

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializeType ResultSerializeType;
    }
}