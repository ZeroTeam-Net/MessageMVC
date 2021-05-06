using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    internal sealed class HttpWriter : IMessageWriter
    {
        public HttpContext Context { get; set; }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        async Task<bool> IMessageWriter.OnResult(IInlineMessage message, object _)
        {
            /*/同步HTTP状态码
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
                 }*/
            // 写入返回
            message.OfflineResult();
            Context.Response.ContentType = "application/json; charset=UTF-8";
            if (!message.IsOutAccess)
            {
                Context.Response.Headers.Add("x-zmvc-id", message.ID);
                Context.Response.Headers.Add("x-zmvc-state", message.State.ToString());
            }
            //else
            //{
            //    Context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            //    Context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
            //    Context.Response.Headers.Add("Access-Control-Allow-Origin","*");
            //}
            await Context.Response.WriteAsync(message.Result ?? "", Encoding.UTF8);
            return true;
        }

    }
}