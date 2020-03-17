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
    public class ApiExecuter
    {
        /// <summary>
        /// 当前站点
        /// </summary>
        internal ZeroService Station;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal ApiCallItem Item;

        /// <summary>
        /// 取消停牌
        /// </summary>
        internal CancellationTokenSource CancellationToken = new CancellationTokenSource();

        /// <summary>
        /// 范围资源
        /// </summary>
        internal IDisposable ScopeResource { get; set; }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="station"></param>
        /// <param name="message"></param>
        public static void OnMessagePush(ZeroService station, string message)
        {
            station.NetPool.Commit();
            MessageItem frame = null;
            try
            {
                frame = JsonHelper.DeserializeObject<MessageItem>(message);
            }
            catch (Exception e)
            {
                station.NetPool.OnError(e, "格式错误");
                return;
            }
            Interlocked.Increment(ref station.CallCount);

            try
            {
                var executer = new ApiExecuter
                {
                    Station = station,
                    Item = new ApiCallItem
                    {
                        Service = frame.ServiceName,
                        ApiName = frame.ApiName,
                        Argument = frame.Argument,
                        Context = frame.Context
                    }
                };
                executer.Execute().Wait();
            }
            catch (Exception e)
            {
                station.NetPool.OnError(e, "未处理异常");
                LogRecorder.Exception(e);
            }
        }

        #region 同步

        /// <summary>
        /// 调用 
        /// </summary>
        public async Task Execute()
        {
            try
            {
                using (ScopeResource = IocScope.CreateScope())
                {
                    try
                    {
                        if (CancellationToken.IsCancellationRequested)
                            return;
                        Prepare();
                        GlobalContext.Current.DependencyObjects.Annex(Item);
                        ZeroOperatorStateType state;
                        try
                        {
                            state = LogRecorder.LogMonitor
                                ? await ApiCallByMonitor()
                                : await ApiCallNoMonitor();
                        }
                        catch (Exception ex)
                        {
                            ZeroTrace.WriteException(Station.ServiceName, ex, "ApiCall", Item.ApiName, Item.Argument);
                            Item.Result = ApiResultIoc.InnerErrorJson;
                            state = ZeroOperatorStateType.LocalException;
                        }

                        Station.NetPool.OnResult(state == ZeroOperatorStateType.Ok, Item.Result);
                        End();
                    }
                    catch (ThreadInterruptedException)
                    {
                        ZeroTrace.SystemLog("Timeout", Item.ApiName, Item.Argument, Item.Content, Item.Context);
                    }
                    catch (Exception ex)
                    {
                        Station.NetPool.OnError(ex, "未处理异常");
                        ZeroTrace.SystemLog("Timeout", Item.ApiName, Item.Argument, Item.Content, Item.Context);
                    }
                    finally
                    {
                        CancellationToken.Dispose();
                        CancellationToken = null;
                    }
                    ScopeResource = null;
                }
            }
            catch (Exception ex)
            {
                ZeroTrace.WriteException(Station.ServiceName, ex, "Api Executer", Item.ApiName, Item.Argument);
            }
        }

        private async Task<ZeroOperatorStateType> ApiCallByMonitor()
        {
            using (MonitorScope.CreateScope($"{Station.ServiceName}/{Item.ApiName}"))
            {
                LogRecorder.MonitorTrace(() => $"GlobalId:{Item.GlobalId}");
                LogRecorder.MonitorTrace(() => JsonConvert.SerializeObject(Item, Formatting.Indented));

                ZeroOperatorStateType state = RestoryContext();
                if (state != ZeroOperatorStateType.Ok)
                {
                    LogRecorder.MonitorTrace("Restory context failed");
                    Interlocked.Increment(ref Station.ErrorCount);
                    return state;
                }
                using (MonitorScope.CreateScope("Do"))
                {
                    state = CommandPrepare(true, out var action);
                    if (state == ZeroOperatorStateType.Ok)
                    {
                        object res;
                        if (CancellationToken.IsCancellationRequested)
                            res = ZeroOperatorStateType.Unavailable;
                        else
                        {
                            res = CommandExec(true, action);
                        }

                        state = await CheckCommandResult(res);
                    }
                }

                if (state != ZeroOperatorStateType.Ok)
                    Interlocked.Increment(ref Station.ErrorCount);
                else
                    Interlocked.Increment(ref Station.SuccessCount);

                LogRecorder.MonitorTrace(Item.Result);
                return state;
            }
        }

        private async Task<ZeroOperatorStateType> ApiCallNoMonitor()
        {
            ZeroOperatorStateType state = RestoryContext();
            if (state != ZeroOperatorStateType.Ok)
            {
                Interlocked.Increment(ref Station.ErrorCount);
                return state;
            }

            Prepare();
            state = CommandPrepare(false, out var action);
            if (state == ZeroOperatorStateType.Ok)
            {
                object res;
                if (CancellationToken.IsCancellationRequested)
                    res = ZeroOperatorStateType.Unavailable;
                else
                {
                    GlobalContext.Current.DependencyObjects.Annex(action);
                    GlobalContext.Current.DependencyObjects.Annex(this);
                    res = CommandExec(false, action);
                }

                state = await CheckCommandResult(res);
            }

            if (state != ZeroOperatorStateType.Ok)
                Interlocked.Increment(ref Station.ErrorCount);
            else
                Interlocked.Increment(ref Station.SuccessCount);
            return state;
        }


        #endregion

        #region 注入


        private void Prepare()
        {
            Item.Handlers = Station.CreateHandlers();
            if (Item.Handlers == null)
                return;
            foreach (var p in Item.Handlers)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;
                try
                {
                    p.Prepare(Station, Item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station.ServiceName, e, "PreActions", Item.ApiName);
                }
            }
        }

        private void End()
        {
            if (Item.Handlers == null)
                return;
            foreach (var p in Item.Handlers)
            {
                try
                {
                    p.End(Station, Item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station.ServiceName, e, "EndActions", Item.ApiName);
                }
            }
        }

        #endregion

        #region 执行命令


        /// <summary>
        /// 还原调用上下文
        /// </summary>
        /// <returns></returns>
        private ZeroOperatorStateType RestoryContext()
        {
            try
            {
                //GlobalContext.Current.DependencyObjects.Annex(CancellationToken);
                if (!string.IsNullOrWhiteSpace(Item.Context))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<GlobalContext>(Item.Context));
                }
                GlobalContext.Current.Request.RequestId = Item.RequestId;
                GlobalContext.Current.Request.CallGlobalId = Item.CallId;
                GlobalContext.Current.Request.LocalGlobalId = Item.GlobalId;
                return ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace(() => $"Restory context exception:{e.Message}");
                ZeroTrace.WriteException(Station.ServiceName, e, Item.ApiName, "restory context", Item.Context);
                Item.Result = ApiResultIoc.ArgumentErrorJson;
                Item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }
        }


        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private ZeroOperatorStateType CommandPrepare(bool monitor, out ApiAction action)
        {
            //1 查找调用方法
            if (!Station.ApiActions.TryGetValue(Item.ApiName.Trim(), out action))
            {
                if (monitor)
                    LogRecorder.MonitorTrace(() => $"Error: Action({Item.ApiName}) no find");
                Item.Result = ApiResultIoc.NoFindJson;
                Item.Status = UserOperatorStateType.NotFind;
                return ZeroOperatorStateType.NotFind;
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
                return ZeroOperatorStateType.Ok;
            try
            {
                if (!action.RestoreArgument(Item.Argument ?? "{}"))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace("Error: argument can't restory.");
                    Item.Result = ApiResultIoc.ArgumentErrorJson;
                    Item.Status = UserOperatorStateType.FormalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument restory {e.Message}.");
                ZeroTrace.WriteException(Station.ServiceName, e, Item.ApiName, "restory argument", Item.Argument);
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.Status = UserOperatorStateType.FormalError;
                return ZeroOperatorStateType.LocalException;
            }

            try
            {
                if (!action.Validate(out var message))
                {
                    if (monitor)
                        LogRecorder.MonitorTrace($"Error: argument validate {message}.");
                    Item.Result = JsonHelper.SerializeObject(ApiResultIoc.Ioc.Error(ErrorCode.ArgumentError, message));
                    Item.Status = UserOperatorStateType.LogicalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: argument validate {e.Message}.");
                ZeroTrace.WriteException(Station.ServiceName, e, Item.ApiName, "invalidate argument", Item.Argument);
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
            return ZeroOperatorStateType.Ok;
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private object CommandExec(bool monitor, ApiAction action)
        {
            try
            {
                GlobalContext.Current.DependencyObjects.Annex(Item);
                GlobalContext.Current.DependencyObjects.Annex(this);
                GlobalContext.Current.DependencyObjects.Annex(action);
                return action.Execute();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                if (monitor)
                    LogRecorder.MonitorTrace($"Error: execute {e.Message}.");
                ZeroTrace.WriteException(Station.ServiceName, e, Item.ApiName, "execute", JsonHelper.SerializeObject(Item));
                Item.Result = ApiResultIoc.LocalExceptionJson;
                Item.Status = UserOperatorStateType.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private async Task<ZeroOperatorStateType> CheckCommandResult(object res)
        {
            switch (res)
            {
                case ZeroOperatorStateType state:
                    return state;
                case string str:
                    Item.Result = str;
                    Item.Status = UserOperatorStateType.Success;
                    return ZeroOperatorStateType.Ok;
                case ApiFileResult result:
                    if (result.Status == null)
                        result.Status = new OperatorStatus { InnerMessage = Item.GlobalId };
                    else
                        result.Status.InnerMessage = Item.GlobalId;

                    result.Data = null;
                    Item.Status = result.Success ? UserOperatorStateType.Success : UserOperatorStateType.LogicalError;
                    Item.Result = JsonHelper.SerializeObject(result);
                    return result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
                case IApiResult result:
                    Item.Result = JsonHelper.SerializeObject(result);
                    Item.Status = result.Success ? UserOperatorStateType.Success : UserOperatorStateType.LogicalError;
                    return result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
                case Task task:
                    await task;
                    dynamic tresd = task;
                    return await CheckCommandResult(tresd.Result);
            }
            var name = res?.GetType().ToString();
            if (name?.IndexOf("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", StringComparison.Ordinal) == 0)
            {
                dynamic resd = res;
                return await CheckCommandResult(resd.Result);
            }
            Item.Result = ApiResultIoc.SucceesJson;
            Item.Status = UserOperatorStateType.Success;
            return ZeroOperatorStateType.Ok;
        }

        #endregion
    }
}