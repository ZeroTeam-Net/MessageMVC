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

        async Task<MessageState> Process()
        {
            index = 0;
            State = MessageState.None;
            middlewares = IocHelper.ServiceProvider.GetServices<IMessageMiddleware>().ToArray();
            await Handle();
            return State;
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
            return State = await next.Handle(Service, Message, Tag, Handle);
        }

        /// <summary>
        /// 消息保存
        /// </summary>
        /// <returns></returns>
        Task SaveMessage()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        public static async Task<MessageState> OnMessagePush(IService service, IMessageItem message, object tag = null)
        {
            var process = new MessageProcess
            {
                Service = service,
                Message = message,
                Tag = tag
            };
            await process.SaveMessage();

            service.Transport.Commit();
            return await process.Process();
        }
    }
}