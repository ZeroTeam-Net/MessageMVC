using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel.Composition;
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
        void IAutoRegister.AutoRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
        {
            services.AddSingleton<IZeroOption>(pri => ToolsOption.Instance);
        }
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.LateConfigRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
        {
            services.AddSingleton<IApiActionChecker, ApiActionChecker>();
            //异常处理
            services.AddTransient<IMessageMiddleware, ExceptionMiddleware>();
            //消息存储与异常消息重新消费
            if (ToolsOption.Instance.EnableMessageReConsumer)
            {
                services.AddTransient<IMessageMiddleware, StorageMiddleware>();
                services.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();
            }
            //第三方回执
            if (ToolsOption.Instance.EnableReceipt)
                services.AddTransient<IMessageMiddleware, ReceiptMiddleware>();
            //通过反向代理组件处理计划任务消息发送
            if (ToolsOption.Instance.EnableReverseProxy)
                services.AddSingleton<IMessageMiddleware, ReverseProxyMiddleware>();
            //JWT解析
            if (ToolsOption.Instance.EnableJwtToken)
            {
                services.TryAddTransient<ITokenResolver, JwtTokenResolver>();
                services.AddTransient<IMessageMiddleware, JwtTokenMiddleware>();
            }

            //健康检查
            if (ToolsOption.Instance.EnableHealthCheck)
                services.AddTransient<IMessageMiddleware, HealthCheckMiddleware>();

            //显示
            logger.Information($@"
     健康检查 : 启用
     异常处理 : 启用
GlobalContext : 启用
  JWT令牌解析 : {(ToolsOption.Instance.EnableJwtToken ? "启用" : "关闭")}
     反向代理 : {(ToolsOption.Instance.EnableReverseProxy ? "启用" : "关闭")}
 消费失败重放 : {(ToolsOption.Instance.EnableMessageReConsumer ? "启用" : "关闭")}
     调用回执 : {(ToolsOption.Instance.EnableReceipt ? $"启用({ToolsOption.Instance.ReceiptService}/{ToolsOption.Instance.ReceiptApi})" : "关闭")}
");
        }

    }
}
