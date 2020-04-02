using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class HttpTransfer : NetTransferBase, IRpcTransfer
    {
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
        Task INetTransfer.Close()
        {
            task.SetResult(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.OnCallEnd(IMessageItem message, object tag)
        {
            var context = (HttpContext)tag;
            // 写入返回
            return context.Response.WriteAsync(
                message.Result ?? (message.Result = ApiResultIoc.RemoteEmptyErrorJson),
                Encoding.UTF8);
        }
    }
}