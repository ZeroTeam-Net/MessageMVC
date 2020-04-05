using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// 上下文配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ContextOption
    {
        /// <summary>
        ///     启用调用链跟踪,默认为AppOption中的设置, 可通过远程传递而扩散
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool EnableLinkTrace { get; set; }

        /// <summary>
        /// 当前调用需要回执
        /// </summary>
        /// <remarks>
        ///  回执用于保证离线互访时,调用方发出请求后,不能正确接收返回值时,
        ///  以回执的方式将返回值放一个第三方地址取得结果数据,从而提供业务夏利的可能.
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Receipt { get; set; }

        /// <summary>
        /// 自定义的回执地址,不存在时使用全局回执地址
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiptService { get; set; }

    }
}
