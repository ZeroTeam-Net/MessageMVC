using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tools
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
            //页面信息记录
            if (ToolsOption.Instance.EnablePageInfo)
                services.AddSingleton<IMessageMiddleware, PageInfoMiddleware>();
        }
    }
}
