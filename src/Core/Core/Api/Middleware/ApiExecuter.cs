using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ActionTask = System.Threading.Tasks.TaskCompletionSource<(ZeroTeam.MessageMVC.Messages.MessageState state, object result)>;

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
        internal ILogger logger;

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
            logger = DependencyHelper.LoggerFactory.CreateLogger($"{message.Topic}/{message.Title}");
            Service = service;
            Message = message;
            Tag = tag;
            Message.RealState = MessageState.Processing;
            if (await Handle() || next == null)
                return;
            await next();
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <returns></returns>
        async Task<bool> Handle()
        {
            var action = Service.GetApiAction(Message.Title);
            //1 查找调用方法
            if (action == null)
            {
                FlowTracer.MonitorInfomation("错误: 接口({0})不存在", Message.Title);
                Message.RealState = MessageState.Unhandled;
                return false;
            }
            //2 确定调用方法及对应权限
            if (!ZeroAppOption.Instance.IsOpenAccess
                && (!action.Option.HasFlag(ApiOption.Anymouse))
                && (GlobalContext.User == null
                || GlobalContext.User.UserId == UserInfo.SystemUserId
                || GlobalContext.User.UserId == UserInfo.UnknownUserId))
            {
                FlowTracer.MonitorInfomation("错误: 需要用户登录信息");
                Message.RealState = MessageState.Deny;
                var status = DependencyHelper.GetService<IOperatorStatus>();
                status.Code = OperatorStatusCode.BusinessException;
                status.Message = "拒绝访问";
                Message.ResultData = status;
                return true;
            }
            Message.ResultSerializer = action.ResultSerializer;
            Message.ResultCreater = action.ResultCreater;

            //参数处理
            if (!ArgumentPrepare(action))
            {
                return false;
            }
            GlobalContext.Current.IsDelay = true;
            //方法执行
            GlobalContext.Current.Task = new ActionTask();
            try
            {
                _ = action.Execute(GlobalContext.Current.Task, Message, Service.Serialize);
                await GlobalContext.Current.Task.Task;
                var (state, result) = GlobalContext.Current.Task.Task.Result;
                Message.State = state;
                Message.ResultData = result;
                return false;
            }
            //catch (TaskCanceledException)
            //{
            //    Message.State = MessageState.Cancel;
            //    var status = DependencyHelper.GetService<IOperatorStatus>();
            //    status.Code = OperatorStatusCode.TimeOut;
            //    status.Message = "操作被取消";
            //    Message.ResultData = status;
            //}
            catch (FormatException fe)
            {
                FlowTracer.MonitorDetails(() => $"参数转换出错误, 请检查调用参数是否合适:{fe.Message}");
                Message.RealState = MessageState.FormalError;

                var status = DependencyHelper.GetService<IOperatorStatus>();
                status.Code = OperatorStatusCode.ArgumentError;
                status.Message = "参数转换出错误, 请检查调用参数是否合适";
                Message.ResultData = status;
            }
            catch (MessageArgumentNullException b)
            {
                var msg = $"参数{b.ParamName}不能为空";
                FlowTracer.MonitorDetails(msg);
                Message.RealState = MessageState.FormalError;
                var status = DependencyHelper.GetService<IOperatorStatus>();
                status.Code = OperatorStatusCode.ArgumentError;
                status.Message = msg;
                Message.ResultData = status;
            }
            return false;
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
            try
            {
                if (!action.RestoreArgument(Message))
                {
                    FlowTracer.MonitorInfomation("错误 : 无法还原参数");
                    Message.Result = "错误 : 无法还原参数";
                    Message.RealState = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception ex)
            {
                var msg = $"错误 : 还原参数异常{ex.Message}";
                FlowTracer.MonitorInfomation(msg);
                var status = DependencyHelper.GetService<IOperatorStatus>();
                status.Code = OperatorStatusCode.BusinessException;
                status.Message = msg;
                Message.ResultData = status;
                Message.RealState = MessageState.FormalError;
                return false;
            }

            if (action.Option.HasFlag(ApiOption.DictionaryArgument))
            {
                return true;
            }
            try
            {
                if (action.ValidateArgument(Message, out var status))
                {
                    return true;
                }
                var msg = $"参数校验失败 : {status.Message}";
                FlowTracer.MonitorInfomation(msg);
                Message.ResultData = status;
                Message.Result = SmartSerializer.ToInnerString(status);
                Message.RealState = MessageState.FormalError;
                return false;
            }
            catch (Exception ex)
            {
                var msg = $"错误 : 参数校验异常{ex.Message}";
                FlowTracer.MonitorInfomation(msg);
                var status = DependencyHelper.GetService<IOperatorStatus>();
                status.Code = OperatorStatusCode.ArgumentError;
                status.Message = msg;
                Message.ResultData = status;
                Message.RealState = MessageState.FormalError;
                return false;
            }
        }

        #endregion
    }
}