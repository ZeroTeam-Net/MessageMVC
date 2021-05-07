using System;
using System.Text.Json.Serialization;

namespace ZeroTeam.MessageMVC.WechatEx
{
    /// <summary>
    /// 微信API基本消息
    /// </summary>
    public class WechatResult
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonPropertyName("errcode")]
        public string ErrCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonPropertyName("errmsg")]
        public string ErrMessage { get; set; }

    }
}