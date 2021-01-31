using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tcp
{
    /// <summary>
    /// Http应用
    /// </summary>
    public static class TcpApp
    {
        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcTcp(this IServiceCollection services)
        {
            TcpOption.Instance.LoadOption();
            services.AddSingleton<IFlowMiddleware, TcpServiceMiddleware>();
            services.AddSingleton<IFlowMiddleware, TcpPoster>(pri => TcpPoster.Instance);
            services.AddSingleton<IMessagePoster, TcpPoster>(pri => TcpPoster.Instance);
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcTcpClient(this IServiceCollection services)
        {
            TcpOption.Instance.LoadOption();
            services.AddSingleton<IFlowMiddleware, TcpPoster>(pri => TcpPoster.Instance);
            services.AddSingleton<IMessagePoster, TcpPoster>(pri => TcpPoster.Instance);
        }
        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcTcpServer(this IServiceCollection services)
        {
            TcpOption.Instance.LoadOption();
            services.AddSingleton<IFlowMiddleware, TcpServiceMiddleware>();
        }
    }
}