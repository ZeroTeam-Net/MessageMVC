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
        Task IAutoRegister.AutoRegist(IServiceCollection services)
        {
            //ApiResult构造
            services.AddSingleton<IFlowMiddleware, ConsulFlow>();
            return Task.CompletedTask;
        }
    }
}
