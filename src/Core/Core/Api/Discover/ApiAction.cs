using Agebull.Common;
using Agebull.Common.Ioc;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiAction : IApiAction
    {
        /// <summary>
        ///     Api名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     是合符合API契约规定
        /// </summary>
        public bool IsApiContract { get; private set; }

        #region 权限

        /// <summary>
        ///     访问控制
        /// </summary>
        public ApiAccessOption Access { get; set; }

        /// <summary>
        ///     需要登录
        /// </summary>
        public bool NeedLogin => !Access.HasFlag(ApiAccessOption.Anymouse);

        /// <summary>
        ///     是否公开接口
        /// </summary>
        public bool IsPublic => Access.HasFlag(ApiAccessOption.Public);

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
                        var re = DependencyHelper.Create<IOperatorStatus>();
                        re.Code = code;
                        re.Message = msg;
                        return re;
                    };

                switch (ResultSerializeType)
                {
                    case SerializeType.Json:
                        ResultSerializer = DependencyHelper.Create<IJsonSerializeProxy>();
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
            if (ArgumentType == null || message.ArgumentData != null)
            {
                return true;
            }
            message.ArgumentData = message.GetArgument((int)ArgumentScope, (int)ArgumentSerializeType, ArgumentSerializer, ArgumentType);
            return true;
        }

        /// <summary>
        ///     参数校验
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ValidateArgument(IInlineMessage data, out string message)
        {
            if (ArgumentType == null)
            {
                message = null;
                return true;
            }
            if (data.ArgumentData is IApiArgument arg)
            {
                return arg.Validate(out message);
            }

            if (data.ArgumentData != null || Access.HasFlag(ApiAccessOption.ArgumentCanNil))
            {
                message = null;
                return true;
            }

            message = "参数不能为空";
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
        public Task<(MessageState state, object result)> Execute(IInlineMessage message, ISerializeProxy serializer)
        {
            if (IsAsync)
            {
                return FuncAsync(message, ArgumentSerializer ?? serializer, message.ArgumentData);
            }

            var res = FuncSync(message, ArgumentSerializer ?? serializer, message.ArgumentData);
            return Task.FromResult(res);
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
                    ArgumentSerializer = DependencyHelper.Create<IJsonSerializeProxy>();
                    break;
                case SerializeType.NewtonJson:
                    ArgumentSerializer = new NewtonJsonSerializeProxy();
                    break;
                case SerializeType.Xml:
                    ArgumentSerializer = DependencyHelper.Create<IXmlSerializeProxy>();
                    break;
                case SerializeType.Bson:
                    ArgumentSerializer = DependencyHelper.Create<IBsonSerializeProxy>();
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
