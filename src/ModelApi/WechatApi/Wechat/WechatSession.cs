using System.Text.Json.Serialization;

namespace ZeroTeam.MessageMVC.WechatEx
{

    /// <summary>
    /// 微信登录通过code交换得到的Session数据
    /// </summary>
    /// <remarks>
    /// 正常返回的JSON数据包 :{"openid": "OPENID","session_key": "SESSIONKEY",}
    /// 满足UnionID返回条件时，返回的JSON数据包:{"openid": "OPENID","session_key": "SESSIONKEY","unionid": "UNIONID"}
    /// 错误时返回JSON数据包(示例为Code无效):{"errcode": 40029,"errmsg": "invalid code"}
    /// </remarks>
    public class WechatSession : WechatResult
    {
        /// <summary>
        /// session_key
        /// </summary>
        [JsonPropertyName("session_key")]
        public string SessionKey { get; set; }

        /// <summary>
        /// UnionID
        /// </summary>
        [JsonPropertyName("unionid")]
        public string UnionID { get; set; }

        /// <summary>
        /// OpenID
        /// </summary>
        [JsonPropertyName("openid")]
        public string OpenID { get; set; }

    }
}