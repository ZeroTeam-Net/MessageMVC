using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiDocument : DocumentItem
    {
        /// <summary>
        ///     访问设置
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ApiOption AccessOption { get; set; }

        /// <summary>
        ///     分类
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        /// <summary>
        ///     Api路由地址
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Routes;

        /// <summary>
        ///     承载页面
        /// </summary>

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PageUrl { get; set; }

        /// <summary>
        ///     所在控制器名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ControllerName { get; set; }

        /// <summary>
        ///     所在控制器说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ControllerCaption { get; set; }

        /// <summary>
        ///     是否异步任务
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsAsync { get; set; }

        /// <summary>
        ///     返回值说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ResultInfo { get; set; }

        /// <summary>
        /// 序列化类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SerializeType ResultSerializeType { get; set; }

        /// <summary>
        ///     是否有调用参数
        /// </summary>
        public bool HaseArgument => Arguments != null && Arguments.Count > 0;

        /// <summary>
        ///     是否字典参数
        /// </summary>
        public bool IsDictionaryArgument { get; set; }

        /// <summary>
        ///     参数说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ApiArgument> Arguments { get; set; }

        /// <summary>
        /// 反序列化类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SerializeType ArgumentSerializeType { get; set; }

    }

    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiArgument : TypeDocument
    {
        /// <summary>
        ///     参数类型
        /// </summary>
        public ParameterInfo ParameterInfo { get; set; }
    }
}