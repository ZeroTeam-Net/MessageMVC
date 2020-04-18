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
            if (LogRecorder.LogMonitor = ToolsOption.Instance.EnableMarkPoint)
            {
                LogRecorder.BeginMonitor(DependencyScope.Name);
                LogRecorder.MonitorDetails(() => JsonConvert.SerializeObject(message, Formatting.Indented));
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
            TraceStep root = null;
            if (LogRecorder.LogMonitor)
            {
                LogRecorder.MonitorInfomation("State => {0}", message.State);
                LogRecorder.MonitorDetails(() => $"Result => {message.Result}");
                if (message.Trace != null)
                {
                    var trace = message.Trace;
                    LogRecorder.MonitorDetails(() => $"Trace => {trace.ToJson()}");
                    message.Trace = null;
                }
                root = LogRecorder.EndMonitor();
                LogRecorder.TraceMonitor(root);
            }
            var link = new TraceLinkMessage
            {
                Root = root,
                Message = new MessageItem
                {
                    ID = message.ID,
                    State = message.State,
                    Topic = message.Topic,
                    Title = message.Title,
                    Content = message.Content,
                    Result = message.Result
                }
            };
            if (GlobalContext.CurrentNoLazy != null)
            {
                link.Trace = GlobalContext.Current.Trace;
                if (link.Trace.Context != null)
                {
                    link.Trace.Context.Message = null;
                    link.Trace.Context.Trace = null;
                }
                GlobalContext.Current.Trace = null;
                GlobalContext.Current.Message = null;
            }
            var json = SmartSerializer.ToString(link);
            return MessagePoster.PublishAsync(ToolsOption.Instance.MarkPointName, "post", json);
        }
    }

}