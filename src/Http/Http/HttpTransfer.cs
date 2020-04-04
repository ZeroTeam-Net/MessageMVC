using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
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
        async Task<bool> INetTransfer.OnCallEnd(IMessageItem message, object tag)
        {
            var context = (HttpContext)tag;

            if (GlobalContext.CurrentNoLazy?.Status.LastStatus is ApiResult apiResult)
            {
                message.Result ??= apiResult.Code.ToString();
                switch (apiResult.Code)
                {
                    case DefaultErrorCode.LocalException:
                    case DefaultErrorCode.LogicalError:
                    case DefaultErrorCode.LocalError:
                    case DefaultErrorCode.ArgumentError:
                    case DefaultErrorCode.Success:
                    case DefaultErrorCode.TokenTimeOut:
                        context.Response.StatusCode = 200;
                        break;
                    case DefaultErrorCode.NoFind:
                        context.Response.StatusCode = 404;
                        break;
                    case DefaultErrorCode.Ignore:
                    case DefaultErrorCode.ReTry:
                        context.Response.StatusCode = 404;
                        break;
                    case DefaultErrorCode.DenyAccess:
                    case DefaultErrorCode.TokenUnknow:
                        context.Response.StatusCode = 407;
                        break;
                    case DefaultErrorCode.NoReady:
                    case DefaultErrorCode.Unavailable:
                        context.Response.StatusCode = 503;
                        break;
                    case DefaultErrorCode.RemoteError:
                    case DefaultErrorCode.NetworkError:
                        context.Response.StatusCode = 504;
                        break;
                }
            }
            else
            {
                message.Result ??= message.State.ToString(); 
                switch (message.State)
                {
                    case MessageState.Failed:
                    case MessageState.Success:
                    case MessageState.FormalError:
                        context.Response.StatusCode = 200;
                        break;
                    case MessageState.NoSupper:
                        context.Response.StatusCode = 404;
                        break;
                    case MessageState.NetError:
                    case MessageState.Exception:
                        context.Response.StatusCode = 503;
                        break;
                    case MessageState.Cancel:
                        context.Response.StatusCode = 503;
                        break;
                    default:
                        context.Response.StatusCode = 504;
                        break;
                }
            }
            // 写入返回
            await context.Response.WriteAsync(message.Result, Encoding.UTF8);
            return false;
        }
    }
}