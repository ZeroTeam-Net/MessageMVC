using System.Threading;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    ///   空的数据接收类
    /// </summary>
    public sealed class EmptyReceiver : MessageReceiverBase, IMessageReceiver
    {
        /// <summary>
        /// 构造
        /// </summary>
        public EmptyReceiver() : base(nameof(EmptyReceiver))
        {
            CanLocalTunnel = true;
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(EmptyReceiver);

        private TaskCompletionSource<bool> task;

        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            task = new TaskCompletionSource<bool>();
            return task.Task;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        Task ILifeFlow.Closing()
        {
            task.SetResult(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageWriter.OnResult(IInlineMessage message, object tag)
        {
            return Task.FromResult(true);
        }
    }
}