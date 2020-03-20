using System;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;
using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.ApiDocuments
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiActionInfo : ApiDocument
    {
        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumentType;

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
        public bool IsAsync { get; set; }

        /// <summary>是否有调用参数</summary>
        public int ArgumentFeature;



        /// <summary>有参方法</summary>
        public Func<object, object> Action;
    }
}