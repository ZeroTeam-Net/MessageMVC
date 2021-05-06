using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// 消息投递对象
    /// </summary>
    public interface IMessagePoster : IMessageWorker
    {
        /// <summary>
        /// 投递对象名称
        /// </summary>
        public string PosterName { get; }

        /// <summary>
        /// 取得生命周期对象
        /// </summary>
        /// <returns></returns>
        ILifeFlow GetLife();

        /// <summary>
        /// 是否本地接收者
        /// </summary>
        bool IsLocalReceiver { get; }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> Post(IInlineMessage message);

        /// <summary>
        /// 消息检查
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IInlineMessage CheckMessage(IMessageItem message) => null;
    }
}
