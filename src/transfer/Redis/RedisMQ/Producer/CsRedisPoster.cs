using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis生产者
    /// </summary>
    internal class CsRedisPoster : BackgroundPoster<RedisQueueItem>, IMessagePoster
    {
        public CsRedisPoster()
        {
            Name = nameof(CsRedisPoster);
        }

        protected override async Task<bool> DoPost(RedisQueueItem item)
        {
            var client = RedisFlow.Instance.client;

            await client.PublishAsync(item.Channel, item.Message);
            item.Step = 3;
            return true;
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
                Topic = message.Service,
                Message = SmartSerializer.SerializeMessage(message)
            });
        }
    }
}
