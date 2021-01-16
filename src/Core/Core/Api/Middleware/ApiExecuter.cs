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
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Handle | MessageHandleScope.Prepare;

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
        IApiAction action;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (!string.Equals(service.ServiceName, message.ServiceName, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(true);

            Service = service;
            Message = message;
            Tag = tag;

            action = Service.GetApiAction(Message.Title);
            //1 查找调用方法
            if (action == null)
            {
                FlowTracer.MonitorDetails(() => $"错误: 接口({Message.Title})不存在");
                Message.State = MessageState.Unhandled;
                Message.ResultCreater = ApiResultHelper.State;
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

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
            if (string.Equals(service.ServiceName, message.ServiceName, StringComparison.OrdinalIgnoreCase))
            {
                if (await Handle())
                    return;
            }
            await next();
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <returns></returns>
        async Task<bool> Handle()
        {
            Message.RealState = MessageState.Processing;
            //参数处理
            if (!ArgumentPrepare(action))
            {
                return false;
            }
            Message.ResultSerializer = action.ResultSerializer;
            Message.ResultCreater = action.ResultCreater;
            //扩展检查
            var checkers = DependencyHelper.GetServices<IApiActionChecker>();
            foreach(var checker in checkers)
            {
                if (!checker.Check(action, Message))
                    return false;
            }

            //方法执行
            GlobalContext.Current.IsDelay = true;
            GlobalContext.Current.Task = new ActionTask();
            try
            {
                _ = action.Execute( GlobalContext.Current.Task, Message, Service.Serialize);
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
                FlowTracer.MonitorError(() => $"参数转换出错误, 请检查调用参数是否合适:{fe.Message}");
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
                action.RestoreArgument(Message);
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