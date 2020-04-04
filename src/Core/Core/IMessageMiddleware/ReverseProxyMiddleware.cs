using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
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
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            if (service.ServiceName == message.Topic)
            {
                return await next();
            }
            var producer = MessagePoster.GetService(message.Topic);
            if (producer == null)
            {
                return await next();
            }
            try
            {
                LogRecorder.MonitorTrace("ReverseProxy To..");
                var (state, result) = await producer.Post(message);
                message.Result = result;
                message.State = state;
                await service.Transport.OnMessageResult(Processor, message, tag);
            }
            catch (OperationCanceledException ex)
            {
                LogRecorder.MonitorTrace("Cancel");
                message.State = MessageState.Cancel;
                await service.Transport.OnMessageError(Processor, ex, message, tag);
                return MessageState.Cancel;
            }
            catch (ThreadInterruptedException ex)
            {
                LogRecorder.MonitorTrace("Time out");
                message.State = MessageState.Cancel;
                await service.Transport.OnMessageError(Processor, ex, message, tag);
                return MessageState.Cancel;
            }
            catch (NetTransferException ex)
            {
                message.State = MessageState.NetError;
                await service.Transport.OnMessageError(Processor, ex, message, tag);
                return MessageState.Cancel;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex, message);
                message.State = MessageState.Exception;
                await service.Transport.OnMessageError(Processor, ex, message, tag);
                return MessageState.Exception;
            }
            return message.State = MessageState.Success;
        }
    }
}