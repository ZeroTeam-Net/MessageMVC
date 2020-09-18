using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ActionTask = System.Threading.Tasks.TaskCompletionSource<(ZeroTeam.MessageMVC.Messages.MessageState state, object result)>;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiAction : IApiAction
    {
        #region 基本信息

        /// <summary>
        ///     Api路由名称
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        ///     是合符合API契约规定
        /// </summary>
        public bool IsApiContract { get; private set; }

        #endregion

        #region 权限

        /// <summary>
        ///     访问控制
        /// </summary>
        public ApiOption Option { get; set; }

        /// <summary>
        ///     需要登录
        /// </summary>
        public bool NeedLogin => !Option.HasFlag(ApiOption.Anymouse);

        /// <summary>
        ///     是否公开接口
        /// </summary>
        public bool IsPublic => Option.HasFlag(ApiOption.Public);

        #endregion

        #region 返回

        /// <summary>
        ///    返回值类型
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        /// 反序列化类型
        /// </summary>
        public SerializeType ResultSerializeType { get; set; }

        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        public ISerializeProxy ResultSerializer { get; set; }

        /// <summary>
        ///     返回值构造对象
        /// </summary>
        public Func<int, string, object> ResultCreater { get; set; }

        void CheckResult()
        {
            if (ResultType == typeof(void) || ResultType == null)
            {
                IsAsync = false;
                ResultType = null;
                ResultSerializeType = SerializeType.None;
                return;
            }
            if (ResultType == typeof(Task))
            {
                IsAsync = true;
                ResultType = null;
                ResultSerializeType = SerializeType.None;
                return;
            }
            if (ResultType.IsGenericType && ResultType.IsSubclassOf(typeof(Task)))
            {
                IsAsync = true;
                ResultType = ResultType.GetGenericArguments()[0];
            }
            else
            {
                IsAsync = false;
            }
            if (ResultType.IsValueType)
            {
                //var def = ResultType.InvokeMember(null, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);

                ResultCreater = (code, msg) => msg;
                ResultSerializer = new SerializeProxy();
                ResultSerializeType = SerializeType.Custom;
            }
            else if (ResultType == typeof(string))
            {
                ResultCreater = (code, msg) => msg;
                ResultSerializer = new SerializeProxy();
                ResultSerializeType = SerializeType.Custom;
            }
            else
            {
                if (ResultType.IsSupperInterface(typeof(IOperatorStatus)))
                    ResultCreater = (code, msg) =>
                    {
                        var re = DependencyHelper.GetService<IOperatorStatus>();
                        re.Code = code;
                        re.Message = msg;
                        return re;
                    };

                switch (ResultSerializeType)
                {
                    case SerializeType.Json:
                        ResultSerializer = DependencyHelper.GetService<IJsonSerializeProxy>();
                        break;
                    case SerializeType.NewtonJson:
                        ResultSerializer = new NewtonJsonSerializeProxy();
                        break;
                    case SerializeType.Xml:
                        ResultSerializer = new XmlSerializeProxy();
                        break;
                    case SerializeType.Custom:
                        ResultSerializer = new SerializeProxy();
                        break;
                    case SerializeType.Bson:
                        throw new NotSupportedException($"{ResultSerializeType}序列化方式暂不支持");
                }
            }
        }
        #endregion

        #region 参数

        /// <summary>
        /// 反序列化类型
        /// </summary>
        public SerializeType ArgumentSerializeType { get; set; }

        /// <summary>
        /// 反序列化类型
        /// </summary>
        public ArgumentScope ArgumentScope { get; set; }

        /// <summary>
        ///     参数反序列化对象
        /// </summary>
        public ISerializeProxy ArgumentSerializer { get; set; }

        /// <summary>
        ///     参数名称
        /// </summary>
        public string ArgumentName { get; set; }

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumentType { get; set; }

        /// <summary>
        ///     还原参数
        /// </summary>
        public bool RestoreArgument(IInlineMessage message)
        {
            if (message.ArgumentData != null)
            {
                return true;
            }
            ArgumentSerializer ??= DependencyHelper.GetService<ISerializeProxy>();
            if (Option.HasFlag(ApiOption.DictionaryArgument) || ArgumentType == null)
            {
                if (!Option.HasFlag(ApiOption.CustomContent))
                    message.RestoryContentToDictionary(ArgumentSerializer, true);
                return true;
            }

            message.ArgumentData = message.GetArgument((int)ArgumentScope, (int)ArgumentSerializeType, ArgumentSerializer, ArgumentType);
            return true;
        }

        /// <summary>
        ///     参数校验
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ValidateArgument(IInlineMessage data, out IOperatorStatus status)
        {
            if (ArgumentType == null)
            {
                status = null;
                return true;
            }
            if (data.ArgumentData is IApiArgument arg)
            {
                return arg.Validate(out status);
            }

            if (data.ArgumentData != null || Option.HasFlag(ApiOption.ArgumentCanNil))
            {
                status = null;
                return true;
            }

            status = ApiResultHelper.State(OperatorStatusCode.ArgumentError, "参数不能为空");
            return false;
        }

        #endregion

        #region 执行

        /// <summary>
        ///     是否异步任务
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        ///     执行行为
        /// </summary>
        public Func<IInlineMessage, ISerializeProxy, object, object> Function { get; set; }


        private Func<IInlineMessage, ISerializeProxy, object, (MessageState state, object result)> FuncSync;
        private Func<IInlineMessage, ISerializeProxy, object, Task<(MessageState state, object result)>> FuncAsync;


        /// <summary>
        ///     执行
        /// </summary>
        /// <returns></returns>
        public async Task Execute(ActionTask task, IInlineMessage message, ISerializeProxy serializer)
        {
            await Task.Yield();
            (MessageState state, object result) res;
            try
            {
                if (IsAsync)
                {
                    res = await FuncAsync(message, ArgumentSerializer ?? serializer, message.ArgumentData);
                }
                else
                {
                    res = FuncSync(message, ArgumentSerializer ?? serializer, message.ArgumentData);
                }
                if (task.Task.Status < TaskStatus.RanToCompletion)
                {
                    GlobalContext.Current.IsDelay = false;
                    task.SetResult(res);
                }
                else if (!GlobalContext.Current.IsDelay)//
                {
                    //清理范围
                    DependencyScope.Local.Value.Scope.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (task.Task.Status < TaskStatus.RanToCompletion)
                {
                    GlobalContext.Current.IsDelay = false;
                    task.SetException(ex);
                }
                else if (!GlobalContext.Current.IsDelay)
                {
                    //清理范围
                    DependencyScope.Local.Value.Scope.Dispose();
                }
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            CheckArgument();
            CheckResult();
            BuildFunc();
        }

        private void CheckArgument()
        {
            if (ArgumentType == null || ArgumentType == typeof(void))
            {
                ArgumentSerializeType = SerializeType.None;
            }
            switch (ArgumentSerializeType)
            {
                case SerializeType.Json:
                    ArgumentSerializer = DependencyHelper.GetService<IJsonSerializeProxy>();
                    break;
                case SerializeType.NewtonJson:
                    ArgumentSerializer = new NewtonJsonSerializeProxy();
                    break;
                case SerializeType.Xml:
                    ArgumentSerializer = DependencyHelper.GetService<IXmlSerializeProxy>();
                    break;
                case SerializeType.Bson:
                    ArgumentSerializer = DependencyHelper.GetService<IBsonSerializeProxy>();
                    break;
                case SerializeType.Custom:
                    throw new NotSupportedException($"{ResultSerializeType}序列化方式暂不支持");
            }
        }

        private void BuildFunc()
        {
            if (!IsAsync)
            {
                if (ResultType == null)
                {
                    FuncSync = (msg, seri, arg) =>
                    {
                        Function(msg, seri, arg);
                        return (MessageState.Success, null);
                    };
                    return;
                }
                if (ResultType.IsSupperInterface(typeof(IApiResult)))
                {
                    IsApiContract = true;
                    FuncSync = (msg, seri, arg) =>
                    {
                        var res = Function(msg, seri, arg) as IApiResult;
                        if (GlobalContext.CurrentNoLazy != null)
                        {
                            GlobalContext.Current.Status.LastStatus = res;
                        }
                        return res == null
                            ? (MessageState.Failed, null)
                            : (res.Success ? MessageState.Success : MessageState.Failed, res);
                    };
                    return;
                }
                FuncSync = (msg, seri, arg) =>
                {
                    var res = Function(msg, seri, arg);
                    return (MessageState.Success, res);
                };
                return;
            }
            if (ResultType == null)
            {
                FuncAsync = async (msg, seri, arg) =>
                {
                    var task = (Task)Function(msg, seri, arg);
                    await task;
                    return (MessageState.Success, null);
                };
                return;
            }
            if (ResultType.IsSupperInterface(typeof(IApiResult)))
            {
                IsApiContract = true;
                FuncAsync = async (msg, seri, arg) =>
                {
                    var task = Function(msg, seri, arg) as Task;
                    await task;
                    dynamic dy = task;
                    var res = dy.Result as IApiResult;
                    if (GlobalContext.CurrentNoLazy != null)
                    {
                        GlobalContext.Current.Status.LastStatus = res;
                    }

                    return res == null
                        ? (MessageState.Failed, null)
                        : (res.Success ? MessageState.Success : MessageState.Failed, res);
                };
                return;
            }
            FuncAsync = async (msg, seri, arg) =>
            {
                var task = (Task)Function(msg, seri, arg);
                await task;
                dynamic dy = task;
                return (MessageState.Success, dy.Result);
            };
        }
        #endregion

    }
}
/*
 * 
        /// <summary>
        ///     执行行为
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        ///     执行行为
        /// </summary>
        public Action<object> Action1 { get; set; }

        /// <summary>
        ///     执行行为
        /// </summary>
        public Func<object> Func { get; set; }

/// <summary>
///     Api动作
/// </summary>
public sealed class ApiActionObj2 : ApiAction
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<object> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        return Action();
    }
}
/// <summary>
///     Api动作
/// </summary>
public sealed class ApiAction<TResult> : ApiAction
    where TResult : IApiResult
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<TResult> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        return Action();
    }
}

/// <summary>
///     Api动作
/// </summary>
public sealed class ApiTaskAction<TResult> : ApiAction
    where TResult : IApiResult
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<Task<TResult>> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        var res= Action();

        return res.Result;
    }
}

/// <summary>
///     Api动作
/// </summary>
public sealed class ApiTaskAction2<TResult> : ApiAction
    where TResult : IApiResult
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<object,Task<TResult>> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        var res = Action(Argument);

        return res.Result;
    }
}
/// <summary>
///     Api动作
/// </summary>
public sealed class ApiAction2<TResult> : ApiAction
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<TResult> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        return Action();
    }
}
/// <summary>
///     API标准委托
/// </summary>
/// <param name="argument"></param>
/// <returns></returns>
public delegate IApiResult ApiDelegate1(IApiArgument argument);

/// <summary>
///     API标准委托
/// </summary>
/// <returns></returns>
public delegate IApiResult ApiDelegate();

/// <summary>
///     Api动作
/// </summary>
public sealed class AnyApiAction<TControler> : ApiAction
    where TControler : class, new()
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public MethodInfo Method { get; set; }


    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        if (ArgumenType == null)
        {
            var action = Method.CreateDelegate(typeof(ApiDelegate), new TControler());
            return action.DynamicInvoke() as IApiResult;
        }
        else
        {
            var action = Method.CreateDelegate(typeof(ApiDelegate1), new TControler());
            return action.DynamicInvoke(Argument) as IApiResult;
        }
    }
}

/// <summary>
///     Api动作
/// </summary>
public sealed class ApiAction<TArgument, TResult> : ApiAction
    where TArgument : class, IApiArgument
    where TResult : IApiResult
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<TArgument, TResult> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        return Action((TArgument)Argument);
    }
}

/// <summary>
///     Api动作
/// </summary>
public sealed class ApiAction2<TArgument, TResult> : ApiAction
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<TArgument, TResult> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        return Action((TArgument)Argument);
    }
}
/// <summary>
///     Api动作
/// </summary>
public sealed class ApiActionObj : ApiAction
{
    /// <summary>
    ///     执行行为
    /// </summary>
    public Func<object, object> Action { get; set; }

    /// <summary>
    ///     执行
    /// </summary>
    /// <returns></returns>
    public override object Execute()
    {
        return Action(Argument);
    }
}
}*/
