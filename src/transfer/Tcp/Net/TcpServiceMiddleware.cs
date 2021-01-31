using Agebull.Common.Ioc;
using BeetleX;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Tcp
{

    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class TcpServiceMiddleware : IFlowMiddleware
    {

        IServer server;
        TcpHandler handler;
        ILogger logger;
        int IZeroMiddleware.Level => MiddlewareLevel.General;

        string IZeroDependency.Name => nameof(TcpServiceMiddleware);

        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            if (TcpOption.Instance.Server == null || TcpOption.Instance.Server.Port <= 1024 || TcpOption.Instance.Server.Port >= short.MaxValue)
                return Task.CompletedTask;
            logger = DependencyHelper.LoggerFactory.CreateLogger<TcpServiceMiddleware>();
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
        Task ILifeFlow.Destory()
        {
            server?.Dispose();
            return Task.CompletedTask;
        }
    }
}