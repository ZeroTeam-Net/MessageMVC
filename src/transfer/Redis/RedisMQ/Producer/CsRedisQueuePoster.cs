using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis后台发布
    /// </summary>
    internal class CsRedisQueuePoster : BackgroundPoster<RedisQueueItem>
    {
        protected override async Task<bool> DoPost(RedisQueueItem item)
        {
            var client = RedisFlow.Instance.client;
            if (item.IsEvent)
            {
                await client.PublishAsync(item.Channel, item.Message);
                item.Step = 3;
                return true;
            }
            switch (item.Step)
            {
                case 0:
                    await client.SetAsync($"msg:{item.Channel}:{item.ID}", item.Message);
                    item.Step = 1;
                    break;
                case 1:
                    await client.LPushAsync($"msg:{item.Channel}", item.ID);
                    item.Step = 2;
                    break;
                case 2:
                    await client.PublishAsync(item.Channel, item.ID);
                    item.Step = 3;
                    break;
            }
            return true;
        }

    }
}
