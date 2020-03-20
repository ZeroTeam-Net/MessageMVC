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
        /// <summary>
        /// 调用的内容
        /// </summary>
        internal IMessageItem Message;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal object Tag;

        /// <summary>
        /// 准备
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
            MessageState state;
            try
            {
                state = await Execute();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                service.Transport.OnError(e, new MessageItem
                {
                    State = MessageState.Exception,
                    Result = e.Message
                }, tag);
                state = MessageState.Exception;
            }
            await next();
            return state;
        }

        /// <summary>
        /// 当前站点
        /// </summary>
        internal IService Service;

        #region 同步

        /// <summary>
        /// 调用 
        /// </summary>
        public async Task<MessageState> Execute()
        {
            try
            {
                using (IocScope.CreateScope())
                {
                    try
                    {
                        GlobalContext.Current.DependencyObjects.Annex(Message);
                        var state = LogRecorder.LogMonitor
                                ? await ApiCallByMonitor()
                                : await ApiCallNoMonitor();

                        Service.Transport.OnResult(Message, Tag);
                        return state;
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
                }
            }
            catch (Exception ex)
            {
                ZeroTrace.WriteException(Service.ServiceName, ex, "Api Executer", Message.Title, Message.Content);
                return MessageState.Exception;
            }
        }

        private async Task<MessageState> ApiCallByMonitor()
        {
            using (MonitorScope.CreateScope($"{Service.ServiceName}/{Message.Title}"))
            {
                LogRecorder.MonitorTrace(() => $"MessageId:{Message.ID}");
                LogRecorder.MonitorTrace(() => JsonConvert.SerializeObject(Message, Formatting.Indented));

                if (!RestoryContext())
                {
                    LogRecorder.MonitorTrace("Restory context failed");
                    return Message.State;
                }
                using (MonitorScope.CreateScope("Do"))
                {
                    if (!CommandPrepare(true, out var action))
                        return Message.State;
                    var res = await CommandExec(action);
                    Message.State = res.Item1;
                    Message.Result = res.Item2;
                }

                LogRecorder.MonitorTrace(Message.Result);
                return Message.State;
            }
        }

        private async Task<MessageState> ApiCallNoMonitor()
        {
            if (!RestoryContext())
            {
                return Message.State;
            }
            if (!CommandPrepare(true, out var action))
                return Message.State;
            var res = await CommandExec(action);
            Message.State = res.Item1;
            Message.Result = res.Item2;
            return Message.State;
        }


        #endregion

        #region 执行命令


        /// <summary>
        /// 还原调用上下文
        /// </summary>
        /// <returns></returns>
        private bool RestoryContext()
        {
            try
            {
                //GlobalContext.Current.DependencyObjects.Annex(CancellationToken);
                if (!string.IsNullOrWhiteSpace(Message.Context))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<GlobalContext>(Message.Context));
                }
                else
                {
                    GlobalContext.SetEmpty();
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace(() => $"Restory context exception:{e.Message}");
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "restory context", Message.Context);
                Message.Result = ApiResultIoc.ArgumentErrorJson;
                Message.State = MessageState.FormalError;
                return false;
            }
        }


        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool CommandPrepare(bool monitor, out IApiAction action)
        {
            //1 查找调用方法
            if (!Service.Actions.TryGetValue(Message.Title.Trim(), out action))
            {
                if (monitor)
                    LogRecorder.MonitorTrace(() => $"Error: Action({Message.Title}) no find");
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
                    if (monitor)
                        LogRecorder.MonitorTrace("Error: argument can't restory.");
                    Message.Result = ApiResultIoc.ArgumentErrorJson;
                    Message.State = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument restory {e.Message}.");
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "restory argument", Message.Content);
                Message.Result = ApiResultIoc.LocalExceptionJson;
                Message.State = MessageState.FormalError;
                return false;
            }

            try
            {
                if (!action.Validate(out var message))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace($"Error: argument validate {message}.");
                    Message.Result = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.ArgumentError, message));
                    Message.State = MessageState.Failed;
                    return false;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument validate {e.Message}.");
                ZeroTrace.WriteException(Service.ServiceName, e, Message.Title, "invalidate argument", Message.Content);
                Message.Result = ApiResultIoc.LocalExceptionJson;
                Message.State = MessageState.FormalError;
                return false;
            }
            return true;
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