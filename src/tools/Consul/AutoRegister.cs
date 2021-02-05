using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC.AddIn;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    ///   组件注册
    /// </summary>
    [Export(typeof(IAutoRegister))]
    [ExportMetadata("Symbol", '%')]
    public sealed class AutoRegister : IAutoRegister
    {
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger) =>
            services.AddSingleton<IZeroOption>(pri => ConsulOption.Instance);

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.LateConfigRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
        {

            services.AddSingleton<IFlowMiddleware, ServiceAutoRegister>();
            //services.AddSingleton<IFlowMiddleware>(ConsulEventPoster.Instance);
            //services.AddSingleton<IMessagePoster>(ConsulEventPoster.Instance);

            //ReceiverDiscover.Regist(type =>
            //{
            //    var na = type.GetCustomAttribute<ConsulEventAttribute>();
            //    return na != null ? (true, na.Name) : (false, null);
            //},
            //name => new ConsulEventConsumer()
            //);

        }
    }
}
