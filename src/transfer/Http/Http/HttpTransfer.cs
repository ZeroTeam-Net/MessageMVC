using Microsoft.AspNetCore.Http;
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
    public sealed class HttpTransfer : MessageReceiverBase, IServiceTransfer
    {
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

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        async Task<bool> IMessageReceiver.OnResult(IInlineMessage message, object tag)
        {
            var context = (HttpContext)tag;
            var status = message.RuntimeStatus ?? (message.ResultData as IApiResult);

            if (message.Result == null)
            {
                if (message.ResultData != null)
                    message.Result = message.ResultData.ToJson();
                else if (message.RuntimeStatus != null)
                    message.Result = message.RuntimeStatus.ToJson();
            }
            // 写入返回
            message.OfflineResult(this);
            if (status != null)
            {
                switch (status.Code)
                {
                    case DefaultErrorCode.NoFind:
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
                    case DefaultErrorCode.NetworkError:
                        context.Response.StatusCode = 504;
                        break;
                    default:
                        context.Response.StatusCode = 200;
                        break;
                }
            }
            else
            {
                switch (message.State)
                {
                    case MessageState.Failed:
                    case MessageState.Success:
                    case MessageState.FormalError:
                        context.Response.StatusCode = 200;
                        break;
                    case MessageState.NonSupport:
                        context.Response.StatusCode = 404;
                        break;
                    case MessageState.NetworkError:
                    case MessageState.Error:
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
            if (message.IsOutAccess)
            {
                await context.Response.WriteAsync(message.Result ?? "", Encoding.UTF8);
            }
            else
            {
                await context.Response.WriteAsync(message.ToMessageResult().ToJson(), Encoding.UTF8);
            }
            return true;
        }

    }
}