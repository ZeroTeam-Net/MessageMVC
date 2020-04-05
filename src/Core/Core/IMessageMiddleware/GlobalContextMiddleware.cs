using Agebull.Common;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 上下文对象
    /// </summary>
    public class GlobalContextMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;

        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            if (JsonHelper.TryDeserializeObject<ZeroContext>(message.Trace?.ContextJson, out var ctx))
            {
                GlobalContext.SetContext(ctx);
            }
            else
            {
                GlobalContext.Reset();
            }

            GlobalContext.Current.Message = message;
            if (GlobalContext.Current.User == null)
                GlobalContext.Current.User = GlobalContext.Anymouse;
            if (GlobalContext.Current.Status == null)
                GlobalContext.Current.Status = new ContextStatus();
            if (GlobalContext.Current.Option == null)
                GlobalContext.Current.Option = new ContextOption
                {
                    EnableLinkTrace = ZeroAppOption.Instance.EnableLinkTrace
                };
            if (GlobalContext.Current.Trace == null)
                GlobalContext.Current.Trace = message.Trace ?? TraceInfo.New(message.ID);

            return next();
        }
    }
}
