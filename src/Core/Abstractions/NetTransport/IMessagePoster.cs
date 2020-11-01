using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// 消息投递对象
    /// </summary>
    public interface IMessagePoster : IMessageWorker
    {
        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> Post(IInlineMessage message);
    }
}
