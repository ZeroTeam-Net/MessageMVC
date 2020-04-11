using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
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
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => short.MaxValue;

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
        public async Task Handle(IService service, IInlineMessage message, object tag, Func<Task> next)
        {
            Service = service;
            Message = message;
            Tag = tag;
            var action = Service.GetApiAction(Message.Title);
            //1 查找调用方法
            if (action == null)
            {
                LogRecorder.Trace("Error: action({0}) no find", Message.Title);
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.NoFind);
                Message.State = MessageState.NoSupper;
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (MessageBusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MessageBusinessException($"Action execute err.{message.ServiceName}/{message.ApiName}", ex);
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
            Message.Inline(action.ArgumentSerializer ?? Service.Serialize,
                action.ArgumentType,
                action.ResultSerializer,
                ApiResultHelper.Error);

            //2 确定调用方法及对应权限
            //if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            //{
            //    if (monitor)
            //        LogRecorder.MonitorTrace("Error: Need login user");
            //    Message.Result = ApiResultIoc.DenyAccessJson;
            //    Message.Status = UserOperatorStateType.DenyAccess;
            //    return ZeroOperatorStateType.DenyAccess;
            //}

            //3 参数校验
            if (action.Access.HasFlag(ApiAccessOption.ArgumentIsDefault))
            {
                return true;
            }

            try
            {
                if (!action.RestoreArgument(Message))
                {
                    LogRecorder.Trace("Argument can't restory");
                    if (action.IsApiContract)
                    {
                        Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, "Argument can't restory");
                    }
                    Message.State = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Trace("Argument restory error : {0}", ex.Message);
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, "Argument restory error");
                }
                Message.State = MessageState.FormalError;
                return false;
            }

            try
            {
                if (action.ValidateArgument(Message, out string info))
                {
                    return true;
                }
                LogRecorder.Trace("Argument validate fail : {0}", info);
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, info);
                }

                Message.State = MessageState.FormalError;
                return false;
            }
            catch (Exception ex)
            {
                LogRecorder.Trace("Argument validate error : {0}", ex.Message);
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, "Argument validate error");
                }
                Message.State = MessageState.FormalError;
                return false;
            }
        }

        #endregion
    }
}