using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace Agebull.Common.Logging
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
        void IAutoRegister.AutoRegist(IServiceCollection services, ILogger logger)
        {
            LogOption.LoadOption();
            if (!LogOption.Instance.Enable)
                return;
            services.AddSingleton<IMessageMiddleware, TraceLogMiddleware>();
            DependencyHelper.ResetLoggerFactory(builder =>
            {
                builder.ClearProviders();
                builder.Services.TryAddTransient(provider => ConfigurationHelper.Root);
                var config = ConfigurationHelper.Root.GetSection("Logging");
                builder.AddConfiguration(config);
                if (config.GetValue("console", true))
                    builder.AddConsole();
                builder.Services.AddSingleton<ILoggerProvider, MessageLoggerProvider>();
            });
        }
    }
}
