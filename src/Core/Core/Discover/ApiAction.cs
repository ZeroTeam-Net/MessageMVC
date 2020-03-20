using System;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
                return arg.Validate(out message);
            if (Argument != null || Access.HasFlag(ApiAccessOption.ArgumentCanNil))
            {
                message = null;
                return true;
            }

            message = "参数不能为空";
            return false;
        }

        Func<object, Tuple<MessageState, string>> FuncSync;
        Func<object, Task<Tuple<MessageState, string>>> FuncAsync;

        /// <summary>
        ///     执行
        /// </summary>
        /// <returns></returns>
        public Task<Tuple<MessageState, string>> Execute()
        {
            if (IsAsync)
                return FuncAsync(Argument);
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
                    ResultType = null;
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
                ArgumentConvert = arg => JsonConvert.DeserializeObject(arg, ArgumentType);
            }
        }

        void BuildFunc()
        {
            if (!IsAsync)
            {
                if (ResultType == null)
                {
                    FuncSync = arg =>
                    {
                        Function(arg);
                        return new Tuple<MessageState, string>(MessageState.Success, null);
                    };
                }
                else if (ResultType == typeof(string))
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg);
                        return new Tuple<MessageState, string>(MessageState.Success, (string)res);
                    };
                }
                else if (ResultType.IsSupperInterface(typeof(IApiResult)))
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg) as IApiResult;
                        return res == null
                            ? new Tuple<MessageState, string>(MessageState.Failed, null)
                            : new Tuple<MessageState, string>(res.Success ? MessageState.Success : MessageState.Failed,
                            JsonHelper.SerializeObject(res));
                    };
                }
                else if (ResultType.IsValueType)
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg);
                        return new Tuple<MessageState, string>(MessageState.Success, res.ToString());
                    };
                }
                else
                {
                    FuncSync = arg =>
                    {
                        var res = Function(arg);
                        return new Tuple<MessageState, string>(MessageState.Success, JsonHelper.SerializeObject(res));
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
                    return new Tuple<MessageState, string>(MessageState.Success, null);
                };
            }
            else if (ResultType == typeof(string))
            {
                FuncAsync = async arg =>
                {
                    var task = (Task<string>)Function(arg);
                    await task;
                    return new Tuple<MessageState, string>(MessageState.Success, task.Result);
                };
            }
            else if (ResultType.IsSupperInterface(typeof(IApiResult)))
            {
                FuncAsync = async arg =>
                {
                    var task = Function(arg) as Task;
                    await task;
                    dynamic dy = task;
                    var res = dy.Result as IApiResult;
                    return res == null
                        ? new Tuple<MessageState, string>(MessageState.Failed, null)
                        : new Tuple<MessageState, string>(res.Success ? MessageState.Success : MessageState.Failed,
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
                    return new Tuple<MessageState, string>(MessageState.Success, res);
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
                    return new Tuple<MessageState, string>(MessageState.Success, JsonHelper.SerializeObject(res));
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
