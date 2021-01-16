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
            services.TryAddTransient<IUser, UserInfo>();
            services.AddSingleton<IApiActionChecker, ApiActionChecker>();
            //启用数据与日志记录埋点
            if (ToolsOption.Instance.EnableMarkPoint | FlowTracer.LogMonitor)
            {
                services.AddSingleton<IMessageMiddleware, MarkPointMiddleware>();
            }
            //启用调用链跟踪(使用IZeroContext全局上下文)
            services.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();

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
                services.AddSingleton<ITokenResolver, JwtTokenResolver>();
            
            //健康检查
            services.AddTransient<IMessageMiddleware, HealthCheckMiddleware>();
            //异常处理
            services.AddTransient<IMessageMiddleware, ExceptionMiddleware>();

            //页面信息记录
            if (ToolsOption.Instance.EnablePageInfo)
                services.AddSingleton<IMessageMiddleware, PageInfoMiddleware>();
            
            //显示
            Console.WriteLine($@"-----[工具信息]-----
     健康检查 : 启用
     异常处理 : 启用
GlobalContext : 启用
  JWT令牌解析 : {(ToolsOption.Instance.EnableJwtToken ? "启用" : "关闭")}
     反向代理 : {(ToolsOption.Instance.EnableReverseProxy ? "启用" : "关闭")}
 消费失败重放 : {(ToolsOption.Instance.EnableMessageReConsumer ? "启用" : "关闭")}
     链路追踪 : {(ToolsOption.Instance.EnableLinkTrace ? "启用" : "关闭")}
     跟踪日志 : {(ToolsOption.Instance.EnableMonitorLog ? "启用" : "关闭")}
     数据埋点 : {(ToolsOption.Instance.EnableMarkPoint ? $"启用({ToolsOption.Instance.MarkPointName})" : "关闭")}
     调用回执 : {(ToolsOption.Instance.EnableReceipt ? $"启用({ToolsOption.Instance.ReceiptService}/{ToolsOption.Instance.ReceiptApi})" : "关闭")}
 页面信息记录 : {(ToolsOption.Instance.EnablePageInfo ? $"启用({ToolsOption.Instance.PageInfoService}/{ToolsOption.Instance.PageInfoApi})" : "关闭")}
");
            return Task.FromResult(false);
        }

    }
}
