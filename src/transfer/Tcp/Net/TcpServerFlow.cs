using Agebull.Common.Ioc;
using BeetleX;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tcp
{

    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class TcpServerFlow : IFlowMiddleware, IMessagePoster
    {
        #region IMessagePoster

        ILifeFlow IMessagePoster.GetLife() => null;
        bool IMessagePoster.IsLocalReceiver => true;

        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            message.State = MessageState.ServerMessage;
            await handler.Publish(message);
            return new MessageResult
            {
                ID = message.ID,
                Trace = message.TraceInfo,
                DataState = MessageDataState.ResultOffline,
                State = MessageState.Send
            };
        }

        #endregion
        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        /// 绝对单例
        /// </summary>
        public static TcpServerFlow Instance = new TcpServerFlow();

        IServer server;
        TcpHandler handler;
        ILogger logger;
        int IZeroMiddleware.Level => MiddlewareLevel.General;

        string IZeroDependency.Name => nameof(TcpServerFlow);
        
        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            if (TcpOption.Instance.Server == null || TcpOption.Instance.Server.Port <= 1024 || TcpOption.Instance.Server.Port >= short.MaxValue)
                return Task.CompletedTask;
            MessagePoster.RegistPoster(this,TcpApp.ClientOptionService);
            logger = DependencyHelper.LoggerFactory.CreateLogger<TcpServerFlow>();
            server = SocketFactory.CreateTcpServer<TcpHandler>();
            handler = server.Handler as TcpHandler;
            handler.Logger = logger;
            server.Options.DefaultListen.Port = TcpOption.Instance.Server.Port;
            server.Open();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Open()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Destroy()
        {
            server?.Dispose();
            return Task.CompletedTask;
        }

    }
}