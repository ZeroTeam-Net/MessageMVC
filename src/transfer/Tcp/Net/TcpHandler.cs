using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using BeetleX;
using BeetleX.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Tcp
{
    /// <summary>
    /// Tcp处理器
    /// </summary>
    internal class TcpHandler : ServerHandlerBase
    {
        internal ILogger Logger { get; set; }
        public override void Log(IServer server, ServerLogEventArgs e)
        {
            base.Log(server, e);
        }

        protected override void OnLogToConsole(IServer server, ServerLogEventArgs e)
        {
            LogLevel level;
            if (e.Type.HasFlag(LogType.Fatal))
                level = LogLevel.Critical;
            else if (e.Type.HasFlag(LogType.Error))
                level = LogLevel.Error;
            else if (e.Type.HasFlag(LogType.Warring))
                level = LogLevel.Warning;
            else if (e.Type.HasFlag(LogType.Info))
                level = LogLevel.Information;
            else if (e.Type.HasFlag(LogType.Debug))
                level = LogLevel.Debug;
            else level = LogLevel.Trace;

            Logger.Log(level,e.Message);

        }
        public override void Error(IServer server, ServerErrorEventArgs e)
        {
            if (e.Error != null)
                Logger.Exception(e.Error, e.Message);
            else
                Logger.Error(e.Message);
        }
        protected override void OnReceiveMessage(IServer server, ISession session, object message)
        {
            base.OnReceiveMessage(server, session, message);
        }

        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            OnReceive(e);
            base.SessionReceive(server, e);
        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {
            Logger.Trace(()=>$"远程开始连接:{e.Socket.RemoteEndPoint}");
            base.Connecting(server, e);
        }
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            Logger.Trace(()=>$"远程连接成功:{e.Session.RemoteEndPoint}");
            base.Connected(server, e);
        }
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            Logger.Trace(()=>$"远程连接关闭:{e.Session.RemoteEndPoint}");
            base.Disconnect(server, e);
        }
        private void OnReceive(SessionReceiveEventArgs e)
        {
            TcpOption.Instance.ConcurrencySemaphore.Wait();
            var pipeStream = e.Stream.ToPipeStream();
            
            if (pipeStream.TryReadLine(out var message))
            {
                if (message == null || message.Trim('\0').IsNullOrEmpty())
                {
                    return;
                }
                if (!ZeroAppOption.Instance.IsRuning)
                {
                    try
                    {
                        pipeStream.WriteLine(ApiResultHelper.PauseJson);
                        e.Session.Stream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Logger.Trace(()=>$"TcpHandler.OnReceive:{ex}");
                    }
                }
                else
                {
                    ScopeRuner.RunScope("TcpHandler", OnMessage, (message, e.Session));
                }
            }
        }

        async Task OnMessage((string text, ISession session) arg)
        {
            FlowTracer.BeginMonitor("TcpHandler");
            FlowTracer.MonitorTrace($"【原始消息】：{arg.session.RemoteEndPoint}\n{arg.text}");
            try
            {
                var message = SmartSerializer.ToMessage(arg.text);
                if(message == null)
                {
                    return;
                }
                var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                {
                    ServiceName = message.Service,
                    Receiver = new EmptyReceiver(),
                    Serialize = DependencyHelper.GetService<ISerializeProxy>()
                };

                await MessageProcessor.OnMessagePush(service, message, false, new TcpWriter
                {
                    Session = arg.session
                });
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorDetails($"【发生错误】\n{ex}");
                try
                {
                    var pipeStream = arg.session.Stream.ToPipeStream();
                    pipeStream.WriteLine(ApiResultHelper.UnknowErrorJson);
                    arg.session.Stream.Flush();
                }
                catch (Exception exx)
                {
                    FlowTracer.MonitorDetails($"【又发生错误】\n{exx}");
                }
            }
            finally
            {
                TcpOption.Instance.ConcurrencySemaphore.Release();
            }
        }
    }
}