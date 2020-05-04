namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HttpClient预定义服务映射配置
    /// </summary>
    internal class HttpClientItem
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 基础地址,包含http://
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// UserAgent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 绑定的服务列表,组合结果为 [Url]/[Service]/[ApiName]
        /// </summary>
        public string Services { get; set; }
    }
}
