namespace ZeroTeam.MessageMVC.MessageQueue
{
    /// <summary>
    /// 队列内容
    /// </summary>
    public class QueueItem
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 消息体的JSON
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 备份文本名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 重试次数
        /// </summary>
        public int Try { get; set; }
    }
}
