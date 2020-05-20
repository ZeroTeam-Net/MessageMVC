using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis生产者
    /// </summary>
    internal class CsRedisQueuePoster : MessagePostBase, IMessagePoster
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(CsRedisQueuePoster);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            return RedisBackPoster.Post(message, false);
        }
    }

    /// <summary>
    ///     Redis生产者
    /// </summary>
    internal class CsRedisEventPoster : MessagePostBase, IMessagePoster
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(CsRedisEventPoster);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            return RedisBackPoster.Post(message, true);
        }
    }
}
