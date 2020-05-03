using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
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
            services.TryAddTransient<IApiResultHelper, ApiResultDefault>();
            services.TryAddTransient<IOperatorStatus, OperatorStatus>();
            services.TryAddTransient<IApiResult, ApiResult>();

            return Task.CompletedTask;
        }
    }
}
