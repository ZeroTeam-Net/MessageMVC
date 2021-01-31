using Agebull.Common.Ioc;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// 消息的跟踪辅助类
    /// </summary>
    public static class MessageTraceHelper
    {

        /// <summary>
        /// 构造
        /// </summary>
        public static TraceInfo CreateTraceInfo(this IMessageItem message)
        {
            var ctxMessage = GlobalContext.CurrentNoLazy?.Message;
            var opt = ZeroAppOption.Instance.GetTraceOption(message.Service);
            if (opt.HasFlag(MessageTraceType.LinkTrace))
                return new TraceInfo
                {
                    TraceId = ctxMessage?.ID ?? message.ID,
                    Option = opt,
                    Start = DateTime.Now,
                    LocalId = message.ID,
                    LocalApp = $"{ZeroAppOption.Instance.ShortName ?? ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})",
                    LocalMachine = $"{ZeroAppOption.Instance.HostName}({ZeroAppOption.Instance.LocalIpAddress})"
                };
            return new TraceInfo
            {
                Option = opt,
                Start = DateTime.Now
            };
        }

        /// <summary>
        /// 检查来自请求方的
        /// </summary>
        public static void CheckRequestTraceInfo(this IMessageItem message)
        {
            if (message.TraceInfo.Option.HasFlag(MessageTraceType.Request))
            {
                message.TraceInfo.LocalApp = $"{ZeroAppOption.Instance.ShortName ?? ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
                message.TraceInfo.LocalMachine = $"{ZeroAppOption.Instance.HostName}({ZeroAppOption.Instance.LocalIpAddress})";
            }
        }

        /// <summary>
        /// 检查发送请求的跟踪信息
        /// </summary>
        public static void CheckPostTraceInfo(this IMessageItem message)
        {
            var opttion = ZeroAppOption.Instance.GetTraceOption(message.Service);
            var ctx = GlobalContext.CurrentNoLazy;

            if (opttion.HasFlag(MessageTraceType.Context))
                message.Context = ctx?.ToDictionary();
            if (opttion.HasFlag(MessageTraceType.User))
                message.User = ScopeRuner.ScopeUser?.ToDictionary();

            if (!opttion.AnyFlags(MessageTraceType.LinkTrace, MessageTraceType.Token, MessageTraceType.Headers, MessageTraceType.Request))
            {
                message.TraceInfo = null;
                return;
            }
            var info= message.TraceInfo = new TraceInfo
            {
                Option = opttion,
                Start = DateTime.Now
            };
            var ctxTraceInfo = ctx?.TraceInfo;
            if (opttion.HasFlag(MessageTraceType.LinkTrace))
            {
                if(ctxTraceInfo != null)
                {
                    info.Level = ctxTraceInfo.Level+1;
                    info.TraceId = ctxTraceInfo.TraceId;
                    info.LocalId = message.ID;
                    info.CallId = ctxTraceInfo.LocalId;
                }
                else
                {
                    info.TraceId = message.ID;
                    info.CallId = message.ID;
                    info.LocalId = message.ID;
                }
            }
            if (opttion.HasFlag(MessageTraceType.Request))
            {
                if (ctxTraceInfo != null)
                {
                    info.RequestApp = ctxTraceInfo.RequestApp;
                    info.RequestPage = ctxTraceInfo.RequestPage;
                    info.CallApp = ctxTraceInfo.LocalApp;
                    info.CallMachine = ctxTraceInfo.CallMachine;
                }
                else
                {
                    info.RequestApp = $"{ZeroAppOption.Instance.ShortName ?? ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
                    info.CallApp = $"{ZeroAppOption.Instance.ShortName ?? ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
                    info.CallMachine = $"{ZeroAppOption.Instance.HostName}({ZeroAppOption.Instance.LocalIpAddress})";
                }
            }
            if (opttion.HasFlag(MessageTraceType.Token)&& ctxTraceInfo != null)
                info.Token = ctxTraceInfo.Token;
            if (opttion.HasFlag(MessageTraceType.Headers) && ctxTraceInfo != null)
                info.Headers = ctxTraceInfo.Headers;

        }

        /*
         * 
            

        /// <summary>
        /// 构造
        /// </summary>
        public static void ToRequest(IMessageItem message)
        {
            if (message.Trace == null)
            {
                var opt = ZeroAppOption.Instance.GetTraceOption(message.Service);
                if (!opt.HasFlag(MessageTraceType.LinkTrace))
                {
                    message.Trace = null;
                    return;
                }
                message.Trace = new TraceInfo
                {
                    TraceId = message.ID,
                    Option = opt
                };
            }
            else if (!message.Trace.Option.HasFlag(MessageTraceType.LinkTrace))
            {
                message.Trace = null;
                return;
            }

            message.Trace.CallId = message.ID;
            message.Trace.CallApp = $"{ZeroAppOption.Instance.ShortName ?? ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
            message.Trace.CallMachine = $"{ZeroAppOption.Instance.HostName}({ZeroAppOption.Instance.LocalIpAddress})";
        }
*/
    }
}

