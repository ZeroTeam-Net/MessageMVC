using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

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
        public MessageProcessor Process { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => short.MaxValue;

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
        public async Task<MessageState> Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            Service = service;
            Message = message;
            Tag = tag;
            try
            {
                if (ZeroFlowControl.Config.EnableGlobalContext)
                {
                    GlobalContext.Current.DependencyObjects.Annex(Message);
                }

                if (CommandPrepare(out IApiAction action))
                {
                    if (ZeroFlowControl.Config.EnableGlobalContext)
                    {
                        GlobalContext.Current.DependencyObjects.Annex(Message);
                        GlobalContext.Current.DependencyObjects.Annex(this);
                        GlobalContext.Current.DependencyObjects.Annex(action);
                    }
                    var (state, result) = await action.Execute();
                    Message.State = state;
                    Message.Result = result;
                }
                await Service.Transport.OnMessageResult(Message, Tag);
            }
            catch (OperationCanceledException ex)
            {
                LogRecorder.MonitorTrace("Cancel");
                Message.State = MessageState.Cancel;
                await Service.Transport.OnMessageError(ex, Message, Tag);
                return MessageState.Cancel;
            }
            catch (ThreadInterruptedException ex)
            {
                LogRecorder.MonitorTrace("Time out");
                Message.State = MessageState.Cancel;
                await Service.Transport.OnMessageError(ex, Message, Tag);
                return MessageState.Cancel;
            }
            catch (NetTransferException ex)
            {
                message.State = MessageState.NetError;
                await service.Transport.OnMessageError(ex, message, tag);
                return MessageState.Cancel;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex, message);
                Message.State = MessageState.Exception;
                await Service.Transport.OnMessageError(ex, Message, Tag);
                return MessageState.Exception;
            }
            if (next != null)
            {
                await next();
            }

            return Message.State;
        }

        #endregion

        #region 执行命令

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool CommandPrepare(out IApiAction action)
        {
            //1 查找调用方法
            if (!Service.Actions.TryGetValue(Message.Title, out action))
            {
                LogRecorder.Trace("Error: Action({0}) no find", Message.Title);
                Message.Result = ApiResultIoc.NoFindJson;
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
                if (!action.RestoreArgument(Message.Content))
                {
                    LogRecorder.Trace("Error: argument can't restory.");
                    Message.Result = ApiResultIoc.ArgumentErrorJson;
                    Message.State = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Trace("Error: argument restory {0}.", e.Message);
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "restory argument", Message.Content);
                Message.Result = ApiResultIoc.LocalExceptionJson;
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
                Message.Result = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.ArgumentError, message));
                Message.State = MessageState.Failed;
                return false;
            }
            catch (Exception e)
            {
                LogRecorder.Trace("Error: argument validate {0}.", e.Message);
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "invalidate argument", Message.Content);
                Message.Result = ApiResultIoc.LocalExceptionJson;
                Message.State = MessageState.FormalError;
                return false;
            }
        }

        #endregion
    }
}