using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///    消息处理器
    /// </summary>
    public class MessageProcessor
    {
        #region 处理入口

        /// <summary>
        /// 消息处理(异步)
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        public static Task OnMessagePush(IService service, IMessageItem message, object tag = null)
        {
            var process = new MessageProcessor
            {
                Service = service,
                Message = message,
                taskCompletionSource = new TaskCompletionSource<MessageState>(),
                Tag = tag
            };
            _ = process.Process();
            return process.taskCompletionSource.Task;
        }

        #endregion

        #region 中间件链式调用

        /// <summary>
        /// 当前站点
        /// </summary>
        internal IService Service;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal IMessageItem Message;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal object Tag;

        /// <summary>
        /// 状态
        /// </summary>
        private MessageState State;
        private IMessageMiddleware[] middlewares;
        private int index = 0;

        private async Task Process()
        {
            using (IocScope.CreateScope($"MessageProcessor : {Message.Topic}/{Message.Title}"))
            {
                index = 0;
                State = MessageState.None;
                middlewares = IocHelper.ServiceProvider.GetServices<IMessageMiddleware>().OrderBy(p => p.Level).ToArray();
                try
                {
                    await Handle();
                    await PushResult();
                }
                catch (OperationCanceledException ex)
                {
                    LogRecorder.MonitorTrace("Cancel");
                    Message.State = MessageState.Cancel;
                    await Service.Transport.OnMessageError(this, ex, Message, Tag);
                }
                catch (ThreadInterruptedException ex)
                {
                    LogRecorder.MonitorTrace("Time out");
                    Message.State = MessageState.Cancel;
                    await Service.Transport.OnMessageError(this, ex, Message, Tag);
                }
                catch (NetTransferException ex)
                {
                    LogRecorder.MonitorTrace(() => $"NetError : {ex.Message}");
                    Message.State = MessageState.NetError;
                    await Service.Transport.OnMessageError(this, ex, Message, Tag);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                    LogRecorder.MonitorTrace(()=>$"UnkonwException : {ex.Message}");
                    await Service.Transport.OnMessageError(this, ex, Message, Tag);
                }
            }
        }

        /// <summary>
        /// 链式处理中间件
        /// </summary>
        /// <returns></returns>
        private async Task Handle()
        {
            if (index >= middlewares.Length)
            {
                return;
            }

            var next = middlewares[index++];
            next.Processor = this;
            await next.Handle(Service, Message, Tag, Handle);
        }
        #endregion

        #region 处理结果返回

        private TaskCompletionSource<MessageState> taskCompletionSource;
        private int isPushed;
        /// <summary>
        /// 结果推到调用处
        /// </summary>
        public async Task PushResult()
        {
            if (Interlocked.Increment(ref isPushed) == 1)
            {
                await Service.Transport.OnMessageResult(this, Message, Tag);
                taskCompletionSource.TrySetResult(Message.State = State);
            }
        }

        #endregion
    }
}