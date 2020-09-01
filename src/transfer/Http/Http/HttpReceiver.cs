using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class HttpReceiver : MessageReceiverBase, IServiceReceiver
    {
        #region 外部调用

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task Call(HttpContext context)
        {
            HttpProtocol.CrosCall(context.Response);
            if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var uri = context.Request.GetUri();
            if (uri.AbsolutePath == "/")
            {
                //response.Redirect("/index.html");
                await context.Response.WriteAsync("Wecome MessageMVC,Lucky every day!", Encoding.UTF8);
                return;
            }
            HttpProtocol.FormatResponse(context.Request, context.Response);
            //DependencyHelper.Update(context.RequestServices);
            try
            {

                //命令
                var data = new HttpMessage();
                //开始调用
                if (data.CheckRequest(context))
                {
                    var service = ZeroFlowControl.GetService(data.ServiceName) ??
                         new ZeroService
                         {
                             ServiceName = "***",
                             Receiver = new HttpReceiver(),
                             Serialize = DependencyHelper.GetService<ISerializeProxy>()
                         };
                    await MessageProcessor.OnMessagePush(service, data, false, context);
                }
                else
                {
                    await context.Response.WriteAsync(ApiResultHelper.NotSupportJson, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                DependencyScope.Logger.Exception(e);
                try
                {
                    await context.Response.WriteAsync(ApiResultHelper.BusinessErrorJson, Encoding.UTF8);
                }
                catch (Exception exception)
                {
                    DependencyScope.Logger.Exception(exception);
                }
            }
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
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    break;
                case MessageState.FrameworkError:
                    context.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    break;
                case MessageState.Deny:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case MessageState.None:
                case MessageState.Accept:
                case MessageState.Send:
                case MessageState.Processing:
                    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                    break;
            }
            // 写入返回
            message.OfflineResult();
            if (!message.IsOutAccess)
            {
                context.Response.Headers.Add("zeroID", message.ID);
                //context.Response.Headers.Add("zeroTrace", SmartSerializer.ToInnerString(message.Trace));
                context.Response.Headers.Add("zeroState", message.State.ToString());
            }
            await context.Response.WriteAsync(message.Result ?? "", Encoding.UTF8);
            return true;
        }
        #endregion

        #region IServiceReceiver

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

        #endregion

    }
}