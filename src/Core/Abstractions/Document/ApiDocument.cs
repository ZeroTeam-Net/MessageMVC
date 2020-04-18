using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        public ApiAccessOption AccessOption { get; set; }

        /// <summary>
        ///     参数名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ArgumentName { get; set; }

        /// <summary>
        ///     参数说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ArgumentInfo { get; set; }

        /// <summary>
        ///     参数说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TypeDocument> Arguments { get; set; }

        /// <summary>
        ///     分类
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        /// <summary>
        ///     返回值说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ResultInfo { get; set; }

        /// <summary>
        ///     Api名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ApiName;

        /// <summary>
        ///     承载页面
        /// </summary>

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PageUrl { get; set; }

        /// <summary>
        ///     所在控制器类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Controller { get; set; }

        /// <summary>
        ///     是否有调用参数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool HaseArgument { get; set; }

        /// <summary>
        ///     是否异步任务
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsAsync { get; set; }


        /// <summary>
        /// 反序列化类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SerializeType ArgumentSerializeType { get; set; }

        /// <summary>
        /// 序列化类型
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SerializeType ResultSerializeType { get; set; }
    }
}