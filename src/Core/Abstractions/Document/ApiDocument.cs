using Newtonsoft.Json;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [DataContract]
        [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiDocument : DocumentItem
    {
        /// <summary>
        ///     访问设置
        /// </summary>
        
        [JsonProperty("access", NullValueHandling = NullValueHandling.Ignore)]
        public ApiAccessOption AccessOption;

        /// <summary>
        ///     参数名称
        /// </summary>
        
        [JsonProperty("argumentName", NullValueHandling = NullValueHandling.Ignore)]
        public string ArgumentName { get; set; }

        /// <summary>
        ///     参数说明
        /// </summary>
        
        [JsonProperty("argument", NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ArgumentInfo;

        /// <summary>
        ///     分类
        /// </summary>
        
        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category;

        /// <summary>
        ///     返回值说明
        /// </summary>
        
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ResultInfo;

        /// <summary>
        ///     Api路由名称
        /// </summary>
        
        [JsonProperty("route", NullValueHandling = NullValueHandling.Ignore)]
        public string RouteName;

        /// <summary>
        ///     Api名称
        /// </summary>
        
        [JsonProperty("api", NullValueHandling = NullValueHandling.Ignore)]

        public string ApiName;

        /// <summary>
        ///     承载页面
        /// </summary>
        
        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]

        public string PageUrl;

    }
}