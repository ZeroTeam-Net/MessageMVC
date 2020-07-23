﻿using System.Threading.Tasks;
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
        int IMessageMiddleware.Level => MiddlewareLevel.Front;


        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Prepare;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (message.Trace?.Context != null)
            {
                GlobalContext.SetContext(message);
                message.Trace.Context = null;
            }
            else
            {
                GlobalContext.Reset();
                GlobalContext.Current.Message = message;
            }

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
                GlobalContext.Current.Trace = TraceInfo.New(message.ID);
            }
            else
            {
                message.Trace.LocalId = message.ID;
                if (!message.IsOutAccess && message.Trace.ContentInfo.HasFlag(TraceInfoType.LinkTrace))
                {
                    message.Trace.LocalApp = $"{ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
                    message.Trace.LocalMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})";
                }
            }
            return Task.FromResult(true);
        }
    }
}
