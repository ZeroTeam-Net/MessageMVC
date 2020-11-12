using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 埋点发出中间件
    /// </summary>
    public class MarkPointMiddleware : IMessageMiddleware
    {

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.Framework;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope =>
            ToolsOption.Instance.EnableMarkPoint || FlowTracer.LogMonitor
                ? MessageHandleScope.Prepare | MessageHandleScope.End
                : MessageHandleScope.None;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (FlowTracer.LogMonitorDebug)
                FlowTracer.MonitorDetails(message.TraceInfo);
            else
                FlowTracer.MonitorInfomation($"Argument => {message.Argument}");

            return Task.FromResult(true);
        }

        /// <summary>
        /// 结果处理
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        async Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            TraceStep root = null;
            if (FlowTracer.LogMonitor)
            {
                FlowTracer.MonitorInfomation(() => $"[{message.State}] => {message.Result}");
                FlowTracer.MonitorDetails(() => $"Trace => {SmartSerializer.ToInnerString(message.Trace)}");
                root = FlowTracer.EndMonitor();
                if (root != null)
                    DependencyScope.Logger.TraceMonitor(root);
            }
            if (!ToolsOption.Instance.EnableMarkPoint ||
                (message.Topic == ToolsOption.Instance.MarkPointName && message.ApiName == "post"))
                return;
            var link = new TraceLinkMessage
            {
                Root = root,
                Trace = GlobalContext.CurrentNoLazy?.Trace,
                Message = new MessageItem
                {
                    ID = message.ID,
                    State = message.State,
                    Topic = message.Topic,
                    Title = message.Title,
                    Content = message.Content,
                    Result = message.Result,
                    Context = GlobalContext.CurrentNoLazy?.ToTransfer()
                }
            };
            var json = SmartSerializer.ToString(link);
            await MessagePoster.PublishAsync(ToolsOption.Instance.MarkPointName, "post", json);
        }
    }

}