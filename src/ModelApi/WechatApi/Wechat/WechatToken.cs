using System.Text.Json.Serialization;

namespace ZeroTeam.MessageMVC.WechatEx
{
    /// <summary>
    /// 微信令牌
    /// </summary>
    public class WechatToken : WechatResult
    {
        /// <summary>
        /// 令牌
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

    }
}