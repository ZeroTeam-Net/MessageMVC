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
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task<MessageState> IMessageMiddleware.Handle(IService service, string message, Func<Task<MessageState>> next)
        {
            MessageItem item = null;
            try
            {
                item = JsonHelper.DeserializeObject<MessageItem>(message);
            }
            catch (Exception e)
            {
                service.Transport.OnError(e, "格式错误");
                return MessageState.FormalError;
            }
            MessageState state;
            try
            {
                var executer = new ApiExecuter
                {
                    Service = service,
                    Item = item
                };
                state = await executer.Execute();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                service.Transport.OnError(e, "未处理异常");
                state = MessageState.Exception;
            }
            await next();
            return state;
        }

        /// <summary>
        /// 当前站点
        /// </summary>
        internal IService Service;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal IMessageItem Item;

        /// <summary>
        /// 取消停牌
        /// </summary>
        internal CancellationTokenSource CancellationToken = new CancellationTokenSource();

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
                        if (CancellationToken.IsCancellationRequested)
                            return MessageState.Cancel;
                        GlobalContext.Current.DependencyObjects.Annex(Item);
                        var state = LogRecorder.LogMonitor
                                ? await ApiCallByMonitor()
                                : await ApiCallNoMonitor();

                        Service.Transport.OnResult(Item);
                        return state;
                    }
                    catch (ThreadInterruptedException)
                    {
                        ZeroTrace.SystemLog("Timeout", Item.Title, Item.Content, Item.Content, Item.Context);
                        return MessageState.Cancel;
                    }
                    catch (Exception ex)
                    {
                        Service.Transport.OnError(ex, "未处理异常");
                        ZeroTrace.SystemLog("Timeout", Item.Title, Item.Content, Item.Content, Item.Context);
                        return MessageState.Exception;
                    }
                    finally
                    {
                        CancellationToken.Dispose();
                        CancellationToken = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ZeroTrace.WriteException(Service.ServiceName, ex, "Api Executer", Item.Title, Item.Content);
                return MessageState.Exception;
            }
        }

        private async Task<MessageState> ApiCallByMonitor()
        {
            using (MonitorScope.CreateScope($"{Service.ServiceName}/{Item.Title}"))
            {
                LogRecorder.MonitorTrace(() => $"MessageId:{Item.ID}");
                LogRecorder.MonitorTrace(() => JsonConvert.SerializeObject(Item, Formatting.Indented));

                if (!RestoryContext())
                {
                    LogRecorder.MonitorTrace("Restory context failed");
                    return Item.State;
                }
                using (MonitorScope.CreateScope("Do"))
                {
                    if (!CommandPrepare(true, out var action))
                        return Item.State;
                    object res;
                    if (CancellationToken.IsCancellationRequested)
                        Item.State = MessageState.Cancel;
                    else
                    {
                        res = CommandExec(true, action);
                        await CheckCommandResult(res);
                    }
                }

                LogRecorder.MonitorTrace(Item.Result);
                return Item.State;
            }
        }

        private async Task<MessageState> ApiCallNoMonitor()
        {
            if (!RestoryContext())
            {
                return Item.State;
            }
            if (!CommandPrepare(true, out var action))
                return Item.State;
            object res;
            if (CancellationToken.IsCancellationRequested)
                Item.State = MessageState.Cancel;
            else
            {
                res = CommandExec(true, action);
                await CheckCommandResult(res);
            }
            return Item.State;
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
                if (!string.IsNullOrWhiteSpace(Item.Context))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<GlobalContext>(Item.Context));
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
                ZeroTrace.WriteException(Service.ServiceName, e, Item.Title, "restory context", Item.Context);
                Item.Result = ApiResultIoc.ArgumentErrorJson;
                Item.State = MessageState.FormalError;
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
            if (!Service.Actions.TryGetValue(Item.Title.Trim(), out action))
            {
                if (monitor)
                    LogRecorder.MonitorTrace(() => $"Error: Action({Item.Title}) no find");
                Item.Result = ApiResultIoc.NoFindJson;
                Item.State = MessageState.NoSupper;
                return false;
            }

            //2 确定调用方法及对应权限
            //if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            //{
            //    if (monitor)
            //        LogRecorder.MonitorTrace("Error: Need login user");
            //    Item.Result = ApiResultIoc.DenyAccessJson;
            //    Item.Status = UserOperatorStateType.DenyAccess;
            //    return ZeroOperatorStateType.DenyAccess;
            //}

            //3 参数校验
            if (action.Access.HasFlag(ApiAccessOption.ArgumentIsDefault))
                return true;
            try
            {
                if (!action.RestoreArgument(Item.Content ?? "{}"))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace("Error: argument can't restory.");
                    Item.Result = ApiResultIoc.ArgumentErrorJson;
                    Item.State = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument restory {e.Message}.");
                ZeroTrace.WriteException(Service.ServiceName, e, Item.Title, "restory argument", Item.Content);
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.State = MessageState.FormalError;
                return false;
            }

            try
            {
                if (!action.Validate(out var message))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace($"Error: argument validate {message}.");
                    Item.Result = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.ArgumentError, message));
                    Item.State = MessageState.Failed;
                    return false;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument validate {e.Message}.");
                ZeroTrace.WriteException(Service.ServiceName, e, Item.Title, "invalidate argument", Item.Content);
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.State = MessageState.FormalError;
                return false;
            }
            return true;
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private object CommandExec(bool monitor, IApiAction action)
        {
            GlobalContext.Current.DependencyObjects.Annex(Item);
            GlobalContext.Current.DependencyObjects.Annex(this);
            GlobalContext.Current.DependencyObjects.Annex(action);
            return action.Execute();
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private async Task CheckCommandResult(object res)
        {
            switch (res)
            {
                case string str:
                    Item.Result = str;
                    Item.State = MessageState.Success;
                    return;
                case ApiFileResult result:
                    result.Data = null;
                    Item.Result = JsonHelper.SerializeObject(result);
                    Item.State = result.Success ? MessageState.Success : MessageState.Failed;
                    return;
                case IApiResult result:
                    Item.Result = JsonHelper.SerializeObject(result);
                    Item.State = result.Success ? MessageState.Success : MessageState.Failed;
                    return;
                case Task task:
                    await task;
                    dynamic tresd = task;
                    await CheckCommandResult(tresd.Result);
                    return;
            }
            //BUG: 检测await生成的代码对象,需要重写
            var name = res?.GetType().ToString();
            if (name?.IndexOf("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", StringComparison.Ordinal) == 0)
            {
                dynamic resd = res;
                await CheckCommandResult(resd.Result);
                return;
            }
            Item.Result = ApiResultIoc.SucceesJson;
            Item.State = MessageState.Success;
        }

        #endregion
    }
}