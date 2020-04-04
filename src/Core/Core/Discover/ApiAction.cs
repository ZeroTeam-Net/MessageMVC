using Agebull.Common;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiAction : IApiAction
    {
        /// <summary>
        ///     参数
        /// </summary>
        public object Argument { get; set; }

        /// <summary>
        ///     Api名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     是合符合API契约规定
        /// </summary>
        public bool IsApiContract { get; private set; }

        /// <summary>
        ///     访问控制
        /// </summary>
        public ApiAccessOption Access { get; set; }

        /// <summary>
        ///     需要登录
        /// </summary>
        public bool NeedLogin => (Access & (ApiAccessOption.Employe | ApiAccessOption.Customer | ApiAccessOption.Business)) > 0;

        /// <summary>
        ///     是否公开接口
        /// </summary>
        public bool IsPublic => Access.HasFlag(ApiAccessOption.Public);

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumentType { get; set; }

        /// <summary>
        ///    返回值类型
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        ///     是否异步任务
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        ///     执行行为
        /// </summary>
        public Func<object, object> Function { get; set; }

        /// <summary>
        ///     参数转换方法
        /// </summary>
        public Func<string, object> ArgumentConvert { get; set; }

        /// <summary>
        ///     还原参数
        /// </summary>
        public virtual bool RestoreArgument(string argument)
        {
            Argument = ArgumentConvert(argument);
            return true;
        }

        /// <summary>
        ///     参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Validate(out string message)
        {
            if (ArgumentType == null)
            {
                message = null;
                return true;
            }
            if (Argument is IApiArgument arg)
            {
                return arg.Validate(out message);
            }

            if (Argument != null || Access.HasFlag(ApiAccessOption.ArgumentCanNil))
            {
                message = null;
                return true;
            }

            message = "参数不能为空";
            return false;
        }

        private Func<object, (MessageState state, string result)> FuncSync;
        private Func<object, Task<(MessageState state, string result)>> FuncAsync;

        /// <summary>
        ///     执行
        /// </summary>
        /// <returns></returns>
        public Task<(MessageState state, string result)> Execute()
        {
            if (IsAsync)
            {
                return FuncAsync(Argument);
            }

            var res = FuncSync(Argument);
            return Task.FromResult(res);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            CheckArgument();
            if (ResultType == typeof(Task))
            {
                IsAsync = true;
                ResultType = null;
            }
            else if (ResultType.IsGenericType && ResultType.IsSubclassOf(typeof(Task)))
            {
                IsAsync = true;
                ResultType = ResultType.GetGenericArguments()[0];
            }
            else
            {
                IsAsync = false;
                if (ResultType == typeof(void))
                {
                    ResultType = null;
                }
            }
            BuildFunc();
        }

        private void CheckArgument()
        {
            if (ArgumentType == null || ArgumentType == typeof(void))
            {
                ArgumentConvert = arg => null;
            }
            else if (ArgumentType == typeof(string))
            {
                ArgumentConvert = arg => arg;
            }
            else if (ArgumentType == typeof(int))
            {
                ArgumentConvert = arg => int.Parse(arg);
            }
            else if (ArgumentType == typeof(long))
            {
                ArgumentConvert = arg => long.Parse(arg);
            }
            else if (ArgumentType == typeof(bool))
            {
                ArgumentConvert = arg => bool.Parse(arg);
            }
            else if (ArgumentType == typeof(decimal))
            {
                ArgumentConvert = arg => decimal.Parse(arg);
            }
            else if (ArgumentType == typeof(float))
            {
                ArgumentConvert = arg => float.Parse(arg);
            }
            else if (ArgumentType == typeof(double))
            {
                ArgumentConvert = arg => double.Parse(arg);
            }
            else
            {
                ArgumentConvert = arg => JsonHelper.DeserializeObject(arg, ArgumentType);
            }
        }

        private void BuildFunc()
        {
            if (!IsAsync)
            {
                if (ResultType == null)
                {
                    FuncSync = arg =>
                    {
                        Function(arg);
                        return (MessageState.Success, null);
                    };
                }
                else if (ResultType == typeof(string))
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg);
                        return (MessageState.Success, (string)res);
                    };
                }
                else if (ResultType.IsSupperInterface(typeof(IApiResult)))
                {
                    IsApiContract = true;
                    FuncSync = arg =>
                    {
                        var res = Function(arg) as IApiResult;
                        if (GlobalContext.CurrentNoLazy != null)
                            GlobalContext.Current.Status.LastStatus = res;
                        return res == null
                            ? (MessageState.Failed, null)
                            : (res.Success ? MessageState.Success : MessageState.Failed, JsonHelper.SerializeObject(res));
                    };
                }
                else if (ResultType.IsValueType)
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg);
                        return (MessageState.Success, res.ToString());
                    };
                }
                else
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg);
                        return (MessageState.Success, JsonHelper.SerializeObject(res));
                    };
                }
                return;
            }
            if (ResultType == null)
            {
                FuncAsync = async arg =>
                {
                    var task = (Task)Function(arg);
                    await task;
                    return (MessageState.Success, null);
                };
            }
            else if (ResultType == typeof(string))
            {
                FuncAsync = async arg =>
                {
                    var task = (Task<string>)Function(arg);
                    await task;
                    return (MessageState.Success, task.Result);
                };
            }
            else if (ResultType.IsSupperInterface(typeof(IApiResult)))
            {
                IsApiContract = true;
                FuncAsync = async arg =>
                {
                    var task = Function(arg) as Task;
                    await task;
                    dynamic dy = task;
                    var res = dy.Result as IApiResult;
                    if (GlobalContext.CurrentNoLazy != null)
                        GlobalContext.Current.Status.LastStatus = res;
                    return res == null
                        ? (MessageState.Failed, null)
                        : (res.Success ? MessageState.Success : MessageState.Failed,
                        JsonHelper.SerializeObject(res));
                };
            }
            else if (ResultType.IsValueType)
            {
                FuncAsync = async arg =>
                {
                    var task = (Task)Function(arg);
                    await task;
                    dynamic dy = task;
                    var res = dy.Result.ToString();
                    return (MessageState.Success, res);
                };
            }
            else
            {
                FuncAsync = async arg =>
                {
                    var task = (Task)Function(arg);
                    await task;
                    dynamic dy = task;
                    var res = dy.Result;
                    return (MessageState.Success, JsonHelper.SerializeObject(res));
                };
            }
        }

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
