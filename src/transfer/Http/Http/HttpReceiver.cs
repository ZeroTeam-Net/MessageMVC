using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    internal sealed class HttpReceiver : MessageReceiverBase, IServiceReceiver
    {
        /// <summary>
        /// 构造
        /// </summary>
        public HttpReceiver() : base(nameof(HttpReceiver))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(HttpPoster);

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
            //同步HTTP状态码
            switch (message.State)
            {
                default:
                case MessageState.Failed:
                case MessageState.Success:
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    break;
                case MessageState.NonSupport:
                    context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    break;
                case MessageState.Unhandled:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case MessageState.FormalError:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case MessageState.Cancel:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;
                case MessageState.BusinessError:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
                case MessageState.NetworkError:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case MessageState.FrameworkError:
                    context.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    break;
                case MessageState.Deny:
                    context.Response.StatusCode = (int)HttpStatusCode.NonAuthoritativeInformation;
                    break;
                case MessageState.None:
                case MessageState.Accept:
                case MessageState.Send:
                case MessageState.Processing:
                    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                    break;
            }
            // 写入返回
            string json;
            if (message.IsOutAccess)
            {
                message.OfflineResult();
                json = message.Result;
            }
            else
            {
                json = SmartSerializer.ToString(message.ToMessageResult(true, Service.Serialize));
            }
            await context.Response.WriteAsync(json ?? "", Encoding.UTF8);
            return true;
        }

    }
}