using Agebull.Common;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tools;

namespace ZeroTeam.MessageMVC.Context
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
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Handle;

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
        Task IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task> next)
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
            {
                GlobalContext.Current.User = GlobalContext.Anymouse;
            }

            if (GlobalContext.Current.Status == null)
            {
                GlobalContext.Current.Status = new ContextStatus();
            }

            if (GlobalContext.Current.Option == null)
            {
                GlobalContext.Current.Option = new System.Collections.Generic.Dictionary<string, string>();
                if (ToolsOption.Instance.EnableLinkTrace)
                {
                    GlobalContext.Current.Option.Add("EnableLinkTrace", "true");
                }
            }
            if (GlobalContext.Current.Trace == null)
            {
                GlobalContext.Current.Trace = message.Trace ?? TraceInfo.New(message.ID);
            }

            return next();
        }
    }
}
