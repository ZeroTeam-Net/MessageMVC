using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息投递对象
    /// </summary>
    public interface IMessagePoster : IZeroDependency
    {
        /// <summary>
        /// 是否可用
        /// </summary>
        bool CanDo { get; }

        /// <summary>
        /// 是否本地接收者
        /// </summary>
        bool IsLocalReceiver { get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType State { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        void Initialize() { }

        /// <summary>
        /// 开启
        /// </summary>
        Task Open() => Task.CompletedTask;

        /// <summary>
        /// 关闭
        /// </summary>
        Task Close() => Task.CompletedTask;

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> Post(IInlineMessage message);

    }
}
