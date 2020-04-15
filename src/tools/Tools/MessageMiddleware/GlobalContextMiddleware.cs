using Agebull.Common;
using Agebull.Common.Ioc;
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
        MessageHandleScope IMessageMiddleware.Scope => ToolsOption.Instance.EnableLinkTrace
                ? MessageHandleScope.Handle
                : MessageHandleScope.None;

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
        async Task IMessageMiddleware.Handle(IService service, IInlineMessage message, object tag, Func<Task> next)
        {
            if (message.Trace?.Context != null)
            {
                GlobalContext.SetContext(message.Trace.Context);
                message.Trace.Context = null;
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

            if (message.Trace != null)
            {
                GlobalContext.Current.Trace = message.Trace;
                //message.Trace.Level += 1;
                if (!message.IsOutAccess)
                {
                    message.Trace.LocalId = message.ID;
                    message.Trace.LocalApp = $"{ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
                    message.Trace.LocalMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})";
                }
            }
            if (GlobalContext.Current.Trace == null)
            {
                GlobalContext.Current.Trace = TraceInfo.New(message.ID);
            }
            await next();

        }
    }
}
