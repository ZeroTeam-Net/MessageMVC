using Agebull.Common.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 日志处理中间件
    /// </summary>
    public class LoggerMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => ToolsOption.Instance.EnableMonitorLog
                ? MessageHandleScope.Prepare | MessageHandleScope.End
                : MessageHandleScope.None;

        IDisposable scope;
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (LogRecorder.LogMonitor)
            {
                scope = MonitorScope.CreateScope($"{service.ServiceName}/{message.Title}");
                LogRecorder.MonitorTrace(() => JsonConvert.SerializeObject(message, Formatting.Indented));
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (scope != null)
            {
                LogRecorder.MonitorTrace("[State] {0} [Result]{1}", message.State, message.Result);
                if (message.Trace != null)
                    LogRecorder.MonitorTrace(()=>$"[Trace] {message.Trace.ToJson()}");
                scope.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}