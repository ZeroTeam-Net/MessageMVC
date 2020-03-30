using System;
using ZeroTeam.MessageMVC.ZeroApis;
using System.Threading.Tasks;
using System.Threading;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class HttpTransfer : IRpcTransfer
    {
        IService INetTransfer.Service { get ; set ; }
        string INetTransfer.Name { get ; set ; }

        void IDisposable.Dispose()
        {
        }

        TaskCompletionSource<bool> task;
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