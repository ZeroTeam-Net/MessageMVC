using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis生产者
    /// </summary>
    internal class CsRedisEventPoster : CsRedisQueuePoster, IMessagePoster
    {
        public CsRedisEventPoster()
        {
            Name = nameof(CsRedisEventPoster);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            message.Offline();
            message.RealState = MessageState.AsyncQueue;
            return Post(new RedisQueueItem
            {
                ID = message.ID,
                IsEvent = true,
                Topic = message.Topic,
                Message = SmartSerializer.SerializeMessage(message)
            });
        }
    }
}
