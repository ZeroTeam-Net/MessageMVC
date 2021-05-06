using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tcp
{
    /// <summary>
    /// Http应用
    /// </summary>
    public static class TcpApp
    {
        /// <summary>
        /// 用于客户端接收服务器发送的配置服务名
        /// </summary>
        public const string ClientOptionService = "_TcpClient_";

        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcTcp(this IServiceCollection services)
        {
            TcpOption.haseConsumer = true;
            TcpOption.haseProducer = true;
            services.AddSingleton<IZeroOption>(pri => TcpOption.Instance);
            services.AddSingleton<IFlowMiddleware>(pri => TcpServerFlow.Instance);
            services.AddSingleton<IMessagePoster, TcpPoster>(pri => TcpPoster.Instance);

            ZeroAppOption.Instance.Services.Regist("Tcp", nameof(TcpPoster), () => new EmptyReceiver());

        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void AddMessageMvcTcpClient(this IServiceCollection services)
        {
            TcpOption.haseProducer = true;
            services.AddSingleton<IZeroOption>(pri => TcpOption.Instance);
            services.AddSingleton<IMessagePoster, TcpPoster>(pri => TcpPoster.Instance);
            ZeroAppOption.Instance.Services.Regist("Tcp", nameof(TcpPoster), () => new EmptyReceiver());
        }
    }
}