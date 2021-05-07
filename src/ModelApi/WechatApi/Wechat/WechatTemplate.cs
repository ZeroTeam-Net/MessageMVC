using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.WechatEx
{
    /// <summary>
    /// 微信模板
    /// </summary>
    public class WechatTemplateValue
    {
        /// <summary>
        /// 用户的OpenId
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("touser")]
        public string OpenId { get; set; }

        /// <summary>
        /// 模板标识
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("template_id")]
        public string TemplateId { get; set; }

        /// <summary>
        /// 详情链接
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();
    }
    /// <summary>
    /// 微信模板
    /// </summary>
    public class WechatTemplate
    {
        /// <summary>
        /// 用户的OpenId
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("touser")]
        public string OpenId { get; set; }
        /// <summary>
        /// 模板标识
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("template_id")]
        public string TemplateId { get; set; }
        /// <summary>
        /// 详情链接
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string Url { get; set; }
        /// <summary>
        /// 标题色
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("topcolor")]
        public string TopColor { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public Dictionary<string, WechatTemplateItem> Data { get; } = new Dictionary<string, WechatTemplateItem>();
    }

    /// <summary>
    /// 模板节点
    /// </summary>
    public class WechatTemplateItem
    {
        /// <summary>
        /// 内容
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("value")]
        public string Value { get; set; }
        /// <summary>
        /// 着色
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("color")]
        public string Color { get; set; }
    }
}