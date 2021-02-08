using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using BeetleX.FastHttpApi.Data;
using BeetleX.FastHttpApi.WebSockets;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// HttApi的上下文
    /// </summary>
    public class HttpContext : IHttpContext, IMessageWriter
    {
        #region 对象

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="server"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="dataContext"></param>
        public HttpContext(HttpApiServer server, HttpRequest request, HttpResponse response, IDataContext dataContext)
        {
            Server = server;
            Request = request;
            Response = response;
            Data = dataContext;
        }

        /// <summary>
        /// 请求标识
        /// </summary>
        public string RequestID { get; set; }

        /// <summary>
        /// 请求
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// 返回
        /// </summary>
        public HttpResponse Response { get; set; }


        /// <summary>
        /// 服务
        /// </summary>
        public HttpApiServer Server { get; set; }

        /// <summary>
        /// Section
        /// </summary>
        public ISession Session => Request.Session;

        /// <summary>
        /// 取Session内容
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name] { get => Session[name]; set => Session[name] = value; }

        /// <summary>
        /// 当前Url
        /// </summary>
        public string ActionUrl { get; set; }

        /// <summary>
        /// 上下文数据
        /// </summary>
        public IDataContext Data { get; set; }

        /// <summary>
        /// 写返回值
        /// </summary>
        /// <param name="data"></param>
        public void Result(object data)
        {
            if (data is DataFrame frame)
            {
                frame.Send(Request.Session);
                return;
            }
            if (data is IApiResult result)
            {
                result.RequestId = RequestID;
                frame = Server.CreateDataFrame(result);
            }
            else
            {
                result = ApiResultHelper.Succees(data);
                result.RequestId = RequestID;
                frame = Server.CreateDataFrame();
            }
            frame.Send(Request.Session);
        }
        #endregion

        #region IMessageWriter

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageWriter.OnResult(IInlineMessage message, object _)
        {
            try
            {
                // 写入返回
                message.OfflineResult();
                Response.SetContentType("application/json");

                if (!message.IsOutAccess)
                {
                    Response.Header.Add("x-zmvc-id", message.ID);
                    Response.Header.Add("x-zmvc-state", message.State.ToString());
                }
                Response.Result(message.Result ?? "");
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
                Response.Result(ApiResultHelper.Helper.Serialize(result));
            }
            catch (Exception exception)
            {
                ScopeRuner.ScopeLogger.Exception(exception);
            }
        }
        #endregion

        #region IHttpContext

        object IHttpContext.Tag => null;

        NextQueue IHttpContext.Queue { get; set; }


        bool IHttpContext.WebSocket => true;

        void IHttpContext.Async()
        {
            Response.Async();
        }


        void IHttpContext.SendToWebSocket(DataFrame data, HttpRequest request)
        {
            Server.SendToWebSocket(data, request);
        }

        void IHttpContext.SendToWebSocket(DataFrame data, Func<ISession, HttpRequest, bool> filter)
        {
            Server.SendToWebSocket(data, filter);
        }


        void IHttpContext.SendToWebSocket(ActionResult data, HttpRequest request)
        {
            data.Url ??= ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, request);

        }

        void IHttpContext.SendToWebSocket(ActionResult data, Func<ISession, HttpRequest, bool> filter)
        {
            data.Url ??= ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, filter);
        }
        #endregion
    }
}
