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
        /// 当前处理器
        /// </summary>
        MessageProcessor IMessageMiddleware.Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => -0xFFFFFF;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => ToolsOption.Instance.EnableMarkPoint
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
            if (LogRecorder.LogMonitor = ToolsOption.Instance.EnableLinkTrace)
            {
                LogRecorder.BeginMonitor(DependencyScope.Name);
                LogRecorder.MonitorTrace(() => JsonConvert.SerializeObject(message, Formatting.Indented));
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// 结果处理
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (!LogRecorder.LogMonitor)
                return Task.CompletedTask;

            LogRecorder.MonitorTrace("State => {0}", message.State);
            LogRecorder.MonitorTrace("Result => {0}", message.State);
            var root = LogRecorder.EndMonitor();
            LogRecorder.TraceMonitor(root);
            if (message.Trace != null)
            {
                LogRecorder.MonitorTrace(() => $"Trace => {message.Trace.ToJson()}");
                message.Trace = null;
            }
            return MessagePoster.PublishAsync(ToolsOption.Instance.MarkPointName, "post",
                new TraceLinkMessage
                {
                    Root = root,
                    Message = message as MessageItem,
                    Trace = GlobalContext.CurrentNoLazy?.Trace
                });
        }
    }

}