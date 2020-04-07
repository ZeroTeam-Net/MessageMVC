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
        internal IMessageItem Message;

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
        public async Task Handle(IService service, IMessageItem message, object tag, Func<Task> next)
        {
            Service = service;
            Message = message;
            Tag = tag;
            IApiAction action = null;
            try
            {
                if (CommandPrepare(out action))
                {
                    var (state, result) = await action.Execute();
                    Message.State = state;
                    Message.Result = result;
                }
            }
            catch (OperationCanceledException)
            {
                if (action.IsApiContract)
                {
                    Message.Result = ApiResultHelper.UnavailableJson;
                }

                throw;
            }
            catch (ThreadInterruptedException ex)
            {
                LogRecorder.Exception(ex);
                if (action.IsApiContract)
                {
                    Message.Result = ApiResultHelper.TimeOutJson;
                }

                throw;
            }
            catch (MessageReceiveException ex)
            {
                LogRecorder.Exception(ex);
                if (action.IsApiContract)
                {
                    Message.Result = ApiResultHelper.NetworkErrorJson;
                }

                throw;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                if (action.IsApiContract)
                {
                    Message.Result = ApiResultHelper.LocalExceptionJson;
                }

                throw;
            }
            if (next != null)
            {
                await next();
            }
        }

        #endregion

        #region CommandPrepare

        /// <summary>
        ///    准备,取出方法,参数校验
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool CommandPrepare(out IApiAction action)
        {
            //1 查找调用方法
            if (!Service.Actions.TryGetValue(Message.Title, out action))
            {
                action = new ApiAction();
                LogRecorder.Trace("Error: Action({0}) no find", Message.Title);
                Message.State = MessageState.NoSupper;
                return false;
            }
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
                if (!action.RestoreArgument(Message.GetArgument(action.ArgumentName, action.IsBaseValue)))
                {
                    LogRecorder.Trace("Error: argument can't restory.");
                    if (action.IsApiContract)
                    {
                        Message.Result = ApiResultHelper.ArgumentErrorJson;
                    }

                    Message.State = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Trace("Error: argument restory {0}.", e.Message);
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "restory argument", Message.Content);
                if (action.IsApiContract)
                {
                    Message.Result = ApiResultHelper.LocalExceptionJson;
                }

                Message.State = MessageState.FormalError;
                return false;
            }

            try
            {
                if (action.Validate(out string message))
                {
                    return true;
                }
                LogRecorder.Trace("Error: argument validate {0}.", message);
                if (action.IsApiContract)
                {
                    Message.Result = JsonHelper.SerializeObject(ApiResultHelper.Ioc.Error(DefaultErrorCode.ArgumentError, message));
                }

                Message.State = MessageState.FormalError;
                return false;
            }
            catch (Exception e)
            {
                LogRecorder.Trace("Error: argument validate {0}.", e.Message);
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "invalidate argument", Message.Content);
                if (action.IsApiContract)
                {
                    Message.Result = ApiResultHelper.LocalExceptionJson;
                }

                Message.State = MessageState.FormalError;
                return false;
            }
        }

        #endregion
    }
}