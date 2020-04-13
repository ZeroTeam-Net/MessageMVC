using Agebull.Common.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 反向代理中间件
    /// </summary>
    public class ReverseProxyMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => 0;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Handle;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task IMessageMiddleware.Handle(IService service, IInlineMessage message, object tag, Func<Task> next)
        {
            if (service.ServiceName == message.Topic)
            {
                await next();
                return;
            }
            var producer = MessagePoster.GetService(message.Topic);
            if (producer == null)
            {
                message.RuntimeStatus = ApiResultHelper.Helper.NoFind;
                return;
            }
            message.State = MessageState.Accept;
            LogRecorder.MonitorTrace("ReverseProxy To..");
            try
            {
                var msg = await producer.Post(message);
                msg.GetResult(producer);
                message.CopyResult(msg);
            }
            catch (Exception ex)
            {
                throw new MessageReceiveException(nameof(ReverseProxyMiddleware), ex);
            }
        }
    }
}