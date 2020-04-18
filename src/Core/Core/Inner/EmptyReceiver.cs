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
        }

        string IMessageReceiver.PosterName => null;

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
        Task IMessageReceiver.Close()
        {
            task.SetResult(true);
            return Task.CompletedTask;
        }
    }
}
