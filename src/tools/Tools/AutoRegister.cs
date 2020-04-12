using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Tools;

namespace ZeroTeam.MessageMVC.ConfigSync
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
        void IAutoRegister.AutoRegist(IServiceCollection services)
        {
            //启用跟踪日志
            if (ToolsOption.Instance.EnableMonitorLog)
            {
                services.AddTransient<IMessageMiddleware, LoggerMiddleware>();
            }
            //启用调用链跟踪(使用IZeroContext全局上下文)
            services.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();
            if (ToolsOption.Instance.EnableLinkTrace)
            {
                GlobalContext.EnableLinkTrace = true;
                LogRecorder.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.UserId.ToString() ?? "-1";
                LogRecorder.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Trace?.TraceId ?? RandomCode.Generate(10);
            }
            //启用数据埋点
            if (ToolsOption.Instance.EnableMarkPoint)
            {
                services.AddSingleton<IMessageMiddleware, MarkPointMiddleware>();
            }
            //消息存储与异常消息重新消费
            if (ToolsOption.Instance.EnableMessageReConsumer)
            {
                services.AddTransient<IMessageMiddleware, StorageMiddleware>();
                services.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();
            }
            //第三方回执
            services.AddTransient<IMessageMiddleware, ReceiptMiddleware>();
            //通过反向代理组件处理计划任务消息发送
            services.AddSingleton<IMessageMiddleware, ReverseProxyMiddleware>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void IAutoRegister.Initialize()
        {
            //显示
            Console.WriteLine($@"-----[Tools infomation]-----
    LinkTrace : {(ToolsOption.Instance.EnableLinkTrace ? "Enable" : "Disable")}
   MonitorLog : {(ToolsOption.Instance.EnableMonitorLog ? "Enable" : "Disable")}
 ReceiptSvice : {ToolsOption.Instance.ReceiptService}
   ReConsumer : {(ToolsOption.Instance.EnableMessageReConsumer ? "Enable" : "Disable")}
    MarkPoint : {(ToolsOption.Instance.EnableMarkPoint ? "Enable" : "Disable")}({ToolsOption.Instance.MarkPointName})
");
        }
    }

}
