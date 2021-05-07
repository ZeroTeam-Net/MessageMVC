using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using BeetleX.FastHttpApi.WebSockets;
using System;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.FastHttpApi
{
    public sealed class MessageMvcHttpApiServer : HttpApiServer
    {
        public MessageMvcHttpApiServer()
           : base(LoadOptions())
        {

        }
        protected override void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            try
            {
                //命令
                var reader = new HttpMessageReader();
                var (success, message) = reader.CheckRequest(this, request, response);
                //开始调用
                if (success)
                {
                    var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                    {
                        ServiceName = "***",
                        Receiver = new HttpReceiver(),
                        Serialize = DependencyHelper.GetService<ISerializeProxy>()
                    };
                    MessageProcessor.RunOnMessagePush(service, message, false, response);
                }
                else
                {
                    response.Result(new StringResult
                    {
                        Message = ApiResultHelper.State(OperatorStatusCode.NoFind).ToJson()
                    });
                }
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                try
                {
                    response.Result(new StringResult
                    {
                        Message = ApiResultHelper.State(OperatorStatusCode.BusinessError).ToJson()
                    });
                }
                catch (Exception exception)
                {
                    ScopeRuner.ScopeLogger.Exception(exception);
                }
            }
        }


        protected override void OnWebSocketRequest(HttpRequest request, ISession session, DataFrame data)
        {
            if (Options.WebSocketMaxRps > 0 && session.Count > Options.WebSocketMaxRps)
            {
                FlowTracer.MonitorInfomation(() => $"Websocket {request.ID} {request.RemoteIPAddress} session message queuing exceeds maximum rps!");

                session.Dispose();
                return;
            }
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
            try
            {
                //命令
                var reader = new WebSocketMessageReader();
                var (success, message) = reader.CheckRequest(this, request, data);
                //开始调用
                if (success)
                {
                    var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                    {
                        ServiceName = "***",
                        Receiver = new HttpReceiver(),
                        Serialize = DependencyHelper.GetService<ISerializeProxy>()
                    };
                    MessageProcessor.RunOnMessagePush(service, message, false, request);
                }
                else
                {
                    DataFrame dataFrame = CreateDataFrame(ApiResultHelper.NotSupportJson);
                    request.Session.Send(dataFrame);
                }
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                try
                {
                    DataFrame dataFrame = CreateDataFrame(ApiResultHelper.BusinessErrorJson);
                    request.Session.Send(dataFrame);
                }
                catch (Exception exception)
                {
                    ScopeRuner.ScopeLogger.Exception(exception);
                }
            }
        }

    }
}
