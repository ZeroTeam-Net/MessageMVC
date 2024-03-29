﻿using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 扩展行为检查功能
    /// </summary>
    public class ApiActionChecker : IApiActionChecker
    {
        /// <summary>
        /// 检查接口是否可执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Check(IApiAction action, IInlineMessage message)
        {
            var user = GlobalContext.User;
            //1 确定调用方法及对应权限
            if (ZeroAppOption.Instance.IsOpenAccess || action.Option.HasFlag(ApiOption.Anymouse))
            {
                return true;
            }
            if (!int.TryParse(user[ZeroTeamJwtClaim.Exp], out int last) || DateTime.Now.ToTimestamp() > last)
            {
                FlowTracer.MonitorInfomation(() => $"令牌已过期 => {new DateTime(1970, 1, 1).AddSeconds(last)}");
                message.State = MessageState.Deny;
                message.Result = ApiResultHelper.TokenTimeOutJson;
                return false;
            }
            //1 确定调用方法及对应权限
            if (UserInfo.IsLoginUser(user))
            {
                return true;
            }
            FlowTracer.MonitorInfomation("错误: 需要用户登录信息");

            var status = DependencyHelper.GetService<IOperatorStatus>();
            status.Code = OperatorStatusCode.BusinessException;
            status.Message = "拒绝访问";

            message.RealState = MessageState.Deny;
            message.ResultData = status;
            return false;
        }
    }
}