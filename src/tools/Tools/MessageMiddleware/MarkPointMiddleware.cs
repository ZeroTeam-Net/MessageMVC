using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

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
            ToolsOption.Instance.EnableMarkPoint
                ? MessageHandleScope.End
                : MessageHandleScope.None;

        /// <summary>
        /// 结果处理
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        async Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            var root = FlowTracer.EndMonitor();
            if (root != null)
                ScopeRuner.ScopeLogger.TraceMonitor(root);
            if (ToolsOption.Instance.EnableMarkPoint && (message.Service != ToolsOption.Instance.MarkPointName || message.Method != "post"))
                await MessagePoster.PublishAsync(ToolsOption.Instance.MarkPointName, "post", new TraceLinkMessage
                {
                    Monitor = root,
                    Trace = GlobalContext.CurrentNoLazy?.TraceInfo,
                    Message = new MessageItem
                    {
                        ID = message.ID,
                        State = message.State,
                        Service = message.Service,
                        Method = message.Method,
                        Argument = message.Argument,
                        Result = message.Result,
                        User = GlobalContext.User?.ToDictionary(),
                        Context = GlobalContext.CurrentNoLazy?.ToDictionary()
                    }
                });
        }
    }

}