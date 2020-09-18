using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

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
        public static void AddMessageMvcZeroRpc(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware, ZeroRpcFlow>();
            services.AddSingleton<IFlowMiddleware>(ZeroPostProxy.Instance);
            services.AddSingleton<IServiceReceiver, ZeroRpcReceiver>();
            services.AddSingleton<IMessagePoster, ZeroRPCPoster>();
        }
    }
}
