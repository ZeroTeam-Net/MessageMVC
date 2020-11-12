using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
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
            message.Trace ??= TraceInfo.New(message.ID);
            if (!message.IsOutAccess && message.Trace.ContentInfo.HasFlag(TraceInfoType.LinkTrace))
            {
                message.Trace.LocalApp = $"{ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})";
                message.Trace.LocalMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})";
            }
            var context = GlobalContext.Reset(message);
            IUser user = null;
            if (message.IsOutAccess)
            {
                try
                {
                    var resolver = DependencyHelper.GetService<ITokenResolver>();
                    if (resolver != null)
                    {
                        user = resolver.TokenToUser(message.Trace.Token);
                    }
                }
                catch (Exception ex)
                {
                    FlowTracer.MonitorInfomation(() => $"令牌还原失败 => {message.Trace.Token}\n{ex}");
                }
            }
            else if (message.Context != null && message.Context.TryGetValue("User", out var json))
            {
                user = new UserInfo();
                user.FormJson(json);
            }
            DependencyScope.Dependency.Annex(user);
            FlowTracer.MonitorDetails(() => $"User => {user?.ToJson()}");
            return Task.FromResult(true);
        }
    }
}
