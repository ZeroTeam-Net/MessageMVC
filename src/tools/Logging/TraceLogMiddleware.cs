using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace Agebull.Common.Logging
{
    /// <summary>
    /// 埋点发出中间件
    /// </summary>
    public class TraceLogMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.Last;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => FlowTracer.LogMonitor ? MessageHandleScope.End : MessageHandleScope.None;

        /// <summary>
        /// 结果处理
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            return MessagePoster.PublishAsync(LogOption.Instance.Service, LogOption.Instance.MonitorApi, new TraceLinkMessage
            {
                Monitor = FlowTracer.EndMonitor(false),
                Trace = message.TraceInfo,
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