using Agebull.Common.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 日志处理中间件
    /// </summary>
    public class LoggerMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcess Process { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => -1;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            if (!LogRecorder.LogMonitor)
                return await next();
            using (MonitorScope.CreateScope($"{service.ServiceName}/{message.Title}"))
            {
                LogRecorder.MonitorTrace(() => $"MessageId:{message.ID}");
                LogRecorder.MonitorTrace(() => JsonConvert.SerializeObject(message, Formatting.Indented));

                var state = await next();

                LogRecorder.MonitorTrace(message.Result);

                return state;
            }
        }
    }
}