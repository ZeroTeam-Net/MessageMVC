using System.Text.Json.Serialization;

namespace ZeroTeam.MessageMVC.WechatEx
{
    /// <summary>
    /// 微信返回的用户信息
    /// </summary>
    public class WechatUserInfo : WechatResult
    {
        /// <summary>
        /// 本地的UserId，非微信返回
        /// </summary>
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Nickname
        /// </summary>
        [JsonPropertyName("nickname")]
        public string NickName { get; set; }

        /// <summary>
        /// 是否订阅 1订阅  0取消
        /// </summary>
        [JsonPropertyName("subscribe")]
        public string Subscribe { get; set; }

        /// <summary>
        /// 订阅时间
        /// </summary>
        [JsonPropertyName("subscribe_time")]
        public string SubscribeTime { get; set; }

        /// <summary>
        /// 性别：1男 2女
        /// </summary>
        [JsonPropertyName("sex")]
        public int Sex { get; set; }

        /// <summary>
        /// 性别 男 女
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender => Sex switch
        {
            1 => "男",
            2 => "女",
            _ => "",
        };

        /// <summary>
        /// 语言
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        [JsonPropertyName("province")]
        public string Province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [JsonPropertyName("headimgurl")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// OpenID
        /// </summary>
        [JsonPropertyName("openid")]
        public string OpenID { get; set; }
        
        /// <summary>
        /// UnionID
        /// </summary>
        [JsonPropertyName("unionid")]
        public string UnionID { get; set; }

        /// <summary>
        /// 群组ID
        /// </summary>
        [JsonPropertyName("groupid")]
        public string GroupID { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonPropertyName("remark")]
        public string Remark { get; set; }
    }
}