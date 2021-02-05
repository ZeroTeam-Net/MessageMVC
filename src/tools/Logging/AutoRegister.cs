using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Messages;

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
            services.AddSingleton<IZeroOption>(pri => LogOption.Instance);
        }

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.LateConfigRegist(IServiceCollection services, ILogger logger)
        {
            ZeroAppOption.Instance.TraceOption[LogOption.Instance.Service] = MessageTraceType.None;//不需要链路信息
            services.AddSingleton<IMessageMiddleware, TraceLogMiddleware>();
            DependencyHelper.ResetLoggerFactory(builder =>
            {
                builder.ClearProviders();
                builder.Services.TryAddTransient(provider => ConfigurationHelper.Root);
                var config = ConfigurationHelper.Root.GetSection("Logging");
                builder.AddConfiguration(config);
                if (config.GetValue("Console", true))
                    builder.AddConsole();
                builder.Services.AddSingleton<ILoggerProvider, MessageLoggerProvider>();
            });
        }
    }
}
