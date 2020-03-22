using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Newtonsoft.Json;
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
        async Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            Service = service;
            Message = message;
            Tag = tag;
            try
            {
                GlobalContext.Current.DependencyObjects.Annex(Message);
                if (CommandPrepare(out var action))
                {
                    var res = await CommandExec(action);
                    Message.State = res.Item1;
                    Message.Result = res.Item2;
                }
                Service.Transport.OnResult(Message, Tag);
            }
            catch (OperationCanceledException)
            {
                ZeroTrace.SystemLog("Cancel", Message.Title, Message.Content, Message.Content, Message.Context);
                return MessageState.Cancel;
            }
            catch (ThreadInterruptedException)
            {
                ZeroTrace.SystemLog("Timeout", Message.Title, Message.Content, Message.Content, Message.Context);
                return MessageState.Cancel;
            }
            catch (Exception ex)
            {
                Service.Transport.OnError(ex, new MessageItem
                {
                    State = MessageState.Exception,
                    Result = ex.Message
                }, Tag);
                ZeroTrace.SystemLog("Exception", Message.Title, Message.Content, Message.Content, Message.Context);
                return MessageState.Exception;
            }
            await next();
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
                return true;
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
                if (action.Validate(out var message))
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

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private Task<Tuple<MessageState, string>> CommandExec(IApiAction action)
        {
            GlobalContext.Current.DependencyObjects.Annex(Message);
            GlobalContext.Current.DependencyObjects.Annex(this);
            GlobalContext.Current.DependencyObjects.Annex(action);
            return action.Execute();
        }


        #endregion
    }
}