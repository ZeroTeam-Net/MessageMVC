using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 通过ZMQ实现的进程内通讯
    /// </summary>
    public static class ZeroRPCApp
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        public static void UseZeroRPC(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware, ZeroRpcFlow>();
            services.AddSingleton<IFlowMiddleware, ZeroRPCProxy>();
            services.AddSingleton<IRpcTransfer, ZeroRpcTransport>();
            services.AddSingleton<IMessageProducer, ZeroRPCProducer>();
        }
    }
}
