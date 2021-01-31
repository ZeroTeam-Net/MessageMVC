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
            AsyncPost = RedisOption.Instance.AsyncPost;
        }

        protected override async Task<bool> DoPost(RedisQueueItem item)
        {
            var client = RedisFlow.Instance.client;

            await client.PublishAsync(item.Channel, SmartSerializer.SerializeMessage(item.Message));
            item.Step = 3;
            return true;
        }

    }
}
