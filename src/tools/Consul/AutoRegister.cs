using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
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
        Task<bool> IAutoRegister.AutoRegist(IServiceCollection services)
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

            return Task.FromResult(false);
        }
    }
}
