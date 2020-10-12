using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Web
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
            services.AddTransient<IMessageMiddleware, WebSocketNotify>();
            return Task.FromResult(false);
        }
    }
}
