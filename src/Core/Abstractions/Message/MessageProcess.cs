using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Messages;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///    消息处理器
    /// </summary>
    public class MessageProcess
    {
        #region 处理入口

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        public static Task<MessageState> OnMessagePush(IService service, IMessageItem message, object tag = null)
        {
            var process = new MessageProcess
            {
                Service = service,
                Message = message,
                taskCompletionSource = new TaskCompletionSource<MessageState>(),
                Tag = tag
            };
            Task.Run(process.Process);
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
        MessageState State;

        IMessageMiddleware[] middlewares;

        int index = 0;

        async Task Process()
        {
            using (IocScope.CreateScope())
            {
                index = 0;
                State = MessageState.None;
                middlewares = IocHelper.ServiceProvider.GetServices<IMessageMiddleware>().OrderBy(p => p.Level).ToArray();
                await Handle();
                PushResult();
            }
        }

        /// <summary>
        /// 链式处理中间件
        /// </summary>
        /// <returns></returns>
        async Task<MessageState> Handle()
        {
            if (index >= middlewares.Length)
                return MessageState.None;
            var next = middlewares[index++];
            next.Process = this;
            return State = await next.Handle(Service, Message, Tag, Handle);
        }
        #endregion

        #region 处理结果返回

        TaskCompletionSource<MessageState> taskCompletionSource;

        int isPushed;
        /// <summary>
        /// 结果推到调用处
        /// </summary>
        public void PushResult()
        {
            if (Interlocked.Increment(ref isPushed)==1)
                taskCompletionSource.TrySetResult(Message.State = State);
        }

        /// <summary>
        /// 结果推到调用处
        /// </summary>
        public void PushResult(MessageState state)
        {
            if (Interlocked.Increment(ref isPushed) == 1)
                taskCompletionSource.TrySetResult(Message.State = state);
        }

        #endregion
    }
}