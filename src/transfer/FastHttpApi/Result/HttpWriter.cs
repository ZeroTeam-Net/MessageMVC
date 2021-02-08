using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    internal sealed class HttpWriter : IMessageWriter
    {
        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public bool IsWebSockte { get; set; }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageWriter.OnResult(IInlineMessage message, object _)
        {
            try
            {
                if (IsWebSockte)
                {
                    Request.Session.Send(Request.Server.CreateDataFrame(message.ResultData));
                }
                else
                {
                    if (!message.IsOutAccess)
                    {
                        Response.Header.Add("x-zmvc-id", message.ID);
                        Response.Header.Add("x-zmvc-state", message.State.ToString());
                    }

                    Response.Result(new MessageResult
                    {
                        Message = message 
                    });
                }
            }
            catch (Exception exception)
            {
                ScopeRuner.ScopeLogger.Exception(exception);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        public void WriteResult(IApiResult result)
        {
            try
            {
                var frame = Request.Server.CreateDataFrame(result);
                if (IsWebSockte)
                {
                    Request.Session.Send(frame);
                }
                else
                {
                    Response.Result(new ObjectResult
                    {
                        Message = result
                    });
                    Response.Result(frame);
                }
            }
            catch (Exception exception)
            {
                ScopeRuner.ScopeLogger.Exception(exception);
            }
        }
    }
}