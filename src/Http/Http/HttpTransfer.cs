using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class HttpTransfer : IRpcTransfer
    {
        IService INetTransfer.Service { get; set; }
        string INetTransfer.Name { get; set; }

        void IDisposable.Dispose()
        {
        }

        private TaskCompletionSource<bool> task;
        Task<bool> INetTransfer.Loop(CancellationToken token)
        {
            task = new TaskCompletionSource<bool>();
            return task.Task;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        void INetTransfer.Close()
        {
            task.SetResult(true);
        }

    }
}