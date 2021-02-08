using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///    空接收器
    /// </summary>
    public class EmptyReceiver : MessageReceiverBase, IServiceReceiver, IMessageConsumer, INetEvent
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

    }
}
