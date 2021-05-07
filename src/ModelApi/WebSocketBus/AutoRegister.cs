using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.AddIn;

namespace BeetleX.Zeroteam.WebSocketBus
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
        void IAutoRegister.AutoRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
        {
            services.AddSingleton<IFlowMiddleware, EventProxy>();
        }

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.LateConfigRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
        {
            ZeroFlowControl.Discove(this.GetType().Assembly);
        }
    }
}