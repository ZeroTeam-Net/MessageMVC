using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Messages
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
                //taskCompletionSource = new TaskCompletionSource<MessageState>(),
                Tag = tag
            };
            return process.Process();
            //return process.taskCompletionSource.Task;
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
        /// 所有消息处理中间件
        /// </summary>
        private IMessageMiddleware[] middlewares;

        /// <summary>
        /// 当前中间件序号
        /// </summary>
        private int index = 0;

        private async Task Process()
        {
            using (IocScope.CreateScope($"MessageProcessor : {Message.Topic}/{Message.Title}"))
            {
                index = 0;
                Message.State = MessageState.Accept;
                middlewares = IocHelper.ServiceProvider.GetServices<IMessageMiddleware>().OrderBy(p => p.Level).ToArray();

                foreach (var middleware in middlewares)
                    middleware.Processor = this;
                try
                {
                    await Handle();
                }
                catch (OperationCanceledException ex)
                {
                    Message.State = MessageState.Cancel;
                    Message.Exception = ex;
                    await OnMessageError();
                }
                catch (ThreadInterruptedException ex)
                {
                    Message.State = MessageState.Cancel;
                    Message.Exception = ex;
                    await OnMessageError();
                }
                catch (NetTransferException ex)
                {
                    Message.State = MessageState.NetError;
                    Message.Exception = ex;
                    await OnMessageError();
                }
                catch (Exception ex)
                {
                    Message.State = MessageState.Exception;
                    Message.Exception = ex;
                    await OnMessageError();
                }
                await PushResult();
            }
        }

        /// <summary>
        /// 中间件链式处理
        /// </summary>
        /// <returns></returns>
        private Task Handle()
        {
            while (index < middlewares.Length)
            {
                var next = middlewares[index++];
                if (!next.Scope.HasFlag(MessageHandleScope.Handle))
                    continue;
                return next.Handle(Service, Message, Tag, Handle);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region 处理结果返回

        //private TaskCompletionSource<MessageState> taskCompletionSource;
        //private int isPushed;
        /// <summary>
        /// 结果推到调用处
        /// </summary>
        private async Task PushResult()
        {
            if (Tag == null)//内部自调用,无需处理
                return;
            //if (Interlocked.Increment(ref isPushed) == 1)
            {
                await Service.Transport.OnResult(Message, Tag);
                //taskCompletionSource.TrySetResult(Message.State = State);
            }
        }

        #endregion
        #region 异常处理

        /// <summary>
        /// 错误发生时处理
        /// </summary>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        async Task OnMessageError()
        {
            if (Message.Exception is NetTransferException ne)
            {
                Message.Result = ne.InnerException.Message;
            }
            else
            {
                if (Message.State <= MessageState.Accept)
                    Message.State = MessageState.Exception;
                Message.Result = Message.Exception.Message;
            }
            foreach (var middleware in middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Exception)))
            {
                try
                {
                    await middleware.OnGlobalException(Service, Message, Tag);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
            }
        }
        #endregion

        #region 数据发送结束

        /// <summary>
        /// 数据发送结束
        /// </summary>
        public async Task OnPostEnd()
        {
            foreach (var middleware in middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.End)))
            {
                try
                {
                    await middleware.OnEnd(Message);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
            }
        }
        #endregion
    }
}