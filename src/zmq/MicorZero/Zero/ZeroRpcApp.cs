using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 通过ZeroRpc实现的远程通讯
    /// </summary>
    public static class ZeroRpcApp
    {
        /// <summary>
        /// 使用ZeroRpc
        /// </summary>
        public static void UseZeroRpc(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware, ZeroRpcFlow>();
            services.AddSingleton<IFlowMiddleware, ZeroPostProxy>();
            services.AddSingleton<IRpcTransfer, ZeroRpcTransport>();
            services.AddSingleton<IMessagePoster, ZeroRPCPoster>();
        }
    }
}
