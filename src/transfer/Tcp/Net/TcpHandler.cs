using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using BeetleX;
using BeetleX.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

            Logger.Log(level, e.Message);

        }
        public override void Error(IServer server, ServerErrorEventArgs e)
        {
            if (e.Error != null)
                Logger.Exception(e.Error, e.Message);
            else
                Logger.Error(e.Message);
        }

        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            OnReceive(e);
            base.SessionReceive(server, e);
        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {
            Logger.Trace(() => $"远程开始连接:{e.Socket.RemoteEndPoint}");
            base.Connecting(server, e);
        }
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            Sessions.Add(e.Session.ID, e.Session);
            Logger.Trace(() => $"远程连接成功:{e.Session.RemoteEndPoint}");
            base.Connected(server, e);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            Sessions.Remove(e.Session.ID);
            Logger.Trace(() => $"远程连接关闭:{e.Session.RemoteEndPoint}");
            base.Disconnect(server, e);
        }
        Dictionary<long, ISession> Sessions = new Dictionary<long, ISession>();
        private void OnReceive(SessionReceiveEventArgs e)
        {
            TcpOption.Instance.ConcurrencySemaphore.Wait();
            var pipeStream = e.Stream.ToPipeStream();

            if (!pipeStream.TryReadLine(out var message) || message == null)
            {
                return;
            }
            message = message.Trim('\0').Trim();
            if (message.IsBlank())
            {
                return;
            }
            ScopeRuner.RunScope("TcpHandler", OnMessage, (message, e.Session));
        }

        async Task OnMessage((string text, ISession session) arg)
        {
            FlowTracer.BeginMonitor("TcpHandler");
            FlowTracer.MonitorTrace($"【原始消息】：{arg.session.RemoteEndPoint}\n{arg.text}");
            var writer = new TcpWriter
            {
                Session = arg.session
            };
            try
            {
                var message = SmartSerializer.ToMessage(arg.text);
                if (message == null)
                {
                    return;
                }
                if (!ZeroAppOption.Instance.IsRuning)
                {
                    message.Result = ApiResultHelper.PauseJson;
                    message.State = MessageState.Cancel;
                    await writer.WriteResult(message);
                    return;
                }
                var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                {
                    ServiceName = message.Service,
                    Receiver = new EmptyReceiver(),
                    Serialize = DependencyHelper.GetService<ISerializeProxy>()
                };

                await MessageProcessor.OnMessagePush(service, message, false, writer);
                return;
            }
            finally
            {
                TcpOption.Instance.ConcurrencySemaphore.Release();
            }
        }

        internal async Task Publish(IInlineMessage message)
        {
            ReadOnlyMemory<byte> json = SmartSerializer.ToBytes(message);
            foreach (var session in Sessions.Values)
            {
                try
                {
                    var pipeStream = session.Stream.ToPipeStream();
                    await pipeStream.WriteAsync(json);
                    session.Stream.Flush();
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Publish");
                }
            }
        }
    }
}