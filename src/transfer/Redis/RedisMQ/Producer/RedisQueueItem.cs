using ZeroTeam.MessageMVC.MessageQueue;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// Redis队列
    /// </summary>
    internal class RedisQueueItem : QueueItem
    {
        /// <summary>
        /// 是否事件
        /// </summary>
        public bool IsEvent { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Channel => Topic;

        /// <summary>
        /// 发送步骤
        /// </summary>
        public int Step { get; set; }

    }
}
