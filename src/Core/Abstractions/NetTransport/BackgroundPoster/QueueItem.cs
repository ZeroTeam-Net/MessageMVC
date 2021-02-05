using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

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
        /// 加入时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public IMessageItem Message { get; set; }

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
