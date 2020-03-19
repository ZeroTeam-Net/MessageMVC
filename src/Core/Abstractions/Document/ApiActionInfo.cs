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
        ///     无参方法
        /// </summary>
        public Func<IApiResult> Action;

        /// <summary>
        ///     有参方法
        /// </summary>
        public Func<IApiArgument, IApiResult> ArgumentAction;

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumenType;

        /// <summary>
        ///     所在控制器类型
        /// </summary>
        public string Controller;

        /// <summary>
        ///     是否有调用参数
        /// </summary>
        public bool HaseArgument;
    }
}