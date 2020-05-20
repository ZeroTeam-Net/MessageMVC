namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// Redis队列
    /// </summary>
    internal class RedisQueueItem
    {
        /// <summary>
        /// 消息标识
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 是否事件
        /// </summary>
        public bool IsEvent { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// 序列化后的消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 发送步骤
        /// </summary>
        public int Step { get; set; }
        /// <summary>
        /// 保全文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 重试次数
        /// </summary>
        public int Try { get; set; }
    }
}
