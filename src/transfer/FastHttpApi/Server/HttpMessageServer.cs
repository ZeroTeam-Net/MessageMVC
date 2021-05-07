using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using BeetleX.FastHttpApi.WebSockets;
using System;
using System.IO;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.FastHttpApi
{

    /// <summary>
    /// 消息服务
    /// </summary>
    public sealed class HttpMessageServer : HttpApiServer
    {
        /// <summary>
        /// 构造
        /// </summary>
        public HttpMessageServer()
           : base(HttpMessageOption.Instance.ServerOption)
        {

        }

        readonly object locked = new();
        /// <summary>
        /// Http消息接收处理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected override void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            if (request.Method == HttpParse.OPTIONS_TAG)
            {
                response.Result(new OptionsResult());
            }

            //lock (locked)
            {
                if (OnHttpRequesting(request, response).Cancel)
                {
                    return;
                }
                string baseUrl = request.BaseUrl;
                if (string.IsNullOrEmpty(request.Ext) && baseUrl[^1] != '/')
                {
                    HttpMessageReader.OnHttpRequest(this, request, response);
                }
                else
                {
                    OnProcessResource(request, response);
                }
            }
        }

        /// <summary>
        /// WebSocket消息接收处理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="session"></param>
        /// <param name="data"></param>
        protected override void OnWebSocketRequest(HttpRequest request, ISession session, DataFrame data)
        {
            //if (Options.WebSocketMaxRps > 0 && session.Count > Options.WebSocketMaxRps)
            //{
            //    FlowTracer.MonitorInfomation(() => $"Websocket {request.ID} {request.RemoteIPAddress} session message queuing exceeds maximum rps!");

            //    session.Dispose();
            //    return;
            //}
            FlowTracer.MonitorTrace(() => $"Websocket {request.ID} {request.RemoteIPAddress} receive {data.Type.ToString()}");

            HttpToken token = (HttpToken)session.Tag;
            if (data.Type == DataPacketType.ping)
            {
                DataFrame pong = CreateDataFrame();
                pong.Type = DataPacketType.pong;
                pong.FIN = true;
                session.Send(pong);
                return;
            }
            else if (data.Type == DataPacketType.connectionClose)
            {
                session.Dispose();
                return;
            }
            var writer = new HttpWriter
            {
                Request = request,
                IsWebSockte = true
            };
            try
            {
                //命令
                var reader = new WebSocketMessageReader();
                var (success, message) = reader.CheckRequest(this, request, data);
                //开始调用
                if (success)
                {
                    var service = ZeroFlowControl.GetService(message.Service);
                    if (service == null)
                        writer.WriteResult(ApiResultHelper.Helper.NoFind);
                    else
                        MessageProcessor.RunOnMessagePush(service, message, false, message.HttpContext);
                }
                else
                {
                    writer.WriteResult(ApiResultHelper.Helper.NoFind);
                }
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                writer.WriteResult(ApiResultHelper.Helper.BusinessError);
            }
        }
    }
}
