using Newtonsoft.Json;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Api节点
    /// </summary>
    public class ApiCheckInfo
    {
        /// <summary>
        ///     头
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        ///     是否允许不携带OAuth2.0的令牌
        /// </summary>
        public bool NoBearer => Access < ApiAccessOption.Anymouse;

        /// <summary>
        ///     需要登录
        /// </summary>
        public bool NeedLogin => (((int)Access) & 0xFFF0) > 0;

        /// <summary>
        /// 访问权限
        /// </summary>
        [JsonProperty]
        public ApiAccessOption Access { get; set; }

        /// <summary>
        ///     操作系统
        /// </summary>
        [JsonProperty]
        public string Os { get; set; }

        /// <summary>
        ///     浏览器
        /// </summary>
        [JsonProperty]
        public string App { get; set; }
    }
}