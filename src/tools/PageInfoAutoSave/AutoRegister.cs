using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

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
        Task<bool> IAutoRegister.AutoRegist(IServiceCollection services)
        {
            //页面信息记录
            if (ToolsOption.Instance.EnablePageInfo)
                services.AddSingleton<IMessageMiddleware, PageInfoMiddleware>();
            
            return Task.FromResult(false);
        }

    }
}
