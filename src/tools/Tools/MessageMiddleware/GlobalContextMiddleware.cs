﻿using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tools;
using ZeroTeam.MessageMVC.ZeroApis;

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
            var context = GlobalContext.Reset(message);
            try
            {
                if (!CheckToken(message, context))
                    return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorInfomation(() => $"令牌还原失败 => {message.TraceInfo.Token}\n{ex}");
                return Task.FromResult(false);
            }
            FlowTracer.MonitorDetails(() => $"User => {context.User?.ToJson()}");
            return Task.FromResult(true);
        }

        private static bool CheckToken(IInlineMessage message, IZeroContext context)
        {
            if (message.TraceInfo.Token.IsNullOrEmpty() || !message.IsOutAccess)
            {
                return true;
            }
            var resolver = DependencyHelper.GetService<ITokenResolver>();
            if (resolver == null)
            {
                return true;
            }
            context.User = resolver.TokenToUser(message.TraceInfo.Token);
            if (context.User[ZeroTeamJwtClaim.Iss] != ToolsOption.Instance.JwtIssue)
            {
                FlowTracer.MonitorInfomation(() => $"非法令牌颁发机构 => {context.User[ZeroTeamJwtClaim.Iss]}");
                message.State = MessageState.Deny;
                message.Result = ApiResultHelper.DenyAccessJson;
                return false;
            }
            if (!int.TryParse(context.User[ZeroTeamJwtClaim.Exp], out int last) || DateTime.Now.ToTimestamp() > last)
            {
                FlowTracer.MonitorInfomation(() => $"令牌已过期 => {new DateTime(1970, 1, 1).AddSeconds(last)}");
                message.State = MessageState.Deny;
                message.Result = ApiResultHelper.TokenTimeOutJson;
                return false;
            }
            return true;
        }
    }
}
