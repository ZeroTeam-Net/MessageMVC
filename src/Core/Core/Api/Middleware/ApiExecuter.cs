using Agebull.Common.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     Api调用器
    /// </summary>
    public class ApiExecuter : IMessageMiddleware
    {
        #region 对象

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.General;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Handle;

        /// <summary>
        /// 当前站点
        /// </summary>
        internal IService Service;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal IInlineMessage Message;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal object Tag;

        #endregion

        #region IMessageMiddleware

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag"></param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task IMessageMiddleware.Handle(IService service, IInlineMessage message, object tag, Func<Task> next)
        {
            Service = service;
            Message = message;
            Tag = tag;
            Message.RealState = MessageState.Processing;
            var action = Service.GetApiAction(Message.Title);
            //1 查找调用方法
            if (action == null)
            {
                LogRecorder.MonitorInfomation("错误: 接口({0})不存在", Message.Title);
                Message.RealState = MessageState.Unhandled;
                if (next != null)
                {
                    await next();
                }
                return;
            }
            //2 确定调用方法及对应权限
            if (!ZeroAppOption.Instance.IsOpenAccess
                && (!action.Access.HasFlag(ApiOption.Anymouse))
                && (GlobalContext.User == null || GlobalContext.User.UserId <= UserInfo.SystemOrganizationId))
            {
                LogRecorder.MonitorInfomation("错误: 需要用户登录信息");
                Message.RealState = MessageState.Deny;
                if (next != null)
                {
                    await next();
                }
                return;
            }
            //参数处理
            if (!ArgumentPrepare(action))
            {
                if (next != null)
                {
                    await next();
                }
                return;
            }
            try
            {
                //方法执行
                var (state, result) = await action.Execute(Message, Service.Serialize);
                Message.State = state;
                Message.ResultData = result;
            }
            catch (FormatException)
            {
                Message.RealState = MessageState.FormalError;
                Message.Result = "参数转换出错误, 请检查调用参数是否合适";
            }
            catch (MessageArgumentNullException b)
            {
                Message.RealState = MessageState.FormalError;
                Message.Result = $"参数{b.ParamName}不能为空";
            }

            if (next != null)
            {
                await next();
            }
        }

        #endregion

        #region CommandPrepare

        /// <summary>
        ///    参数校验
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool ArgumentPrepare(IApiAction action)
        {
            //还原参数
            Message.ArgumentInline(
                action.ArgumentSerializer,
                action.Access.HasFlag(ApiOption.DictionaryArgument) ? null : action.ArgumentType,
                action.ResultSerializer,
                action.ResultCreater);


            //3 参数校验
            if (action.Access.HasFlag(ApiOption.DictionaryArgument))
            {
                return true;
            }

            try
            {
                if (!action.RestoreArgument(Message))
                {
                    LogRecorder.MonitorInfomation("错误 : 无法还原参数");
                    Message.Result = "错误 : 无法还原参数";
                    Message.RealState = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception ex)
            {
                var msg = $"错误 : 还原参数异常{ex.Message}";
                LogRecorder.MonitorInfomation(msg);
                Message.Result = msg;
                Message.RealState = MessageState.FormalError;
                return false;
            }

            try
            {
                if (action.ValidateArgument(Message, out string info))
                {
                    return true;
                }
                var msg = $"参数校验失败 : {info}";
                LogRecorder.MonitorInfomation(msg);
                Message.Result = msg;
                Message.RealState = MessageState.FormalError;
                return false;
            }
            catch (Exception ex)
            {
                var msg = $"错误 : 参数校验异常{ex.Message}";
                LogRecorder.MonitorInfomation(msg);
                Message.Result = msg;
                Message.RealState = MessageState.FormalError;
                return false;
            }
        }

        #endregion
    }
}