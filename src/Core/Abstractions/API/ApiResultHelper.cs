using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API返回基类</summary>
    public static class ApiResultHelper
    {
        #region 静态属性

        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        private static IApiResultHelper helper;

        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        public static IApiResultHelper Helper => helper ??= DependencyHelper.GetService<IApiResultHelper>();

        /// <summary>成功的Json字符串</summary>
        /// <remarks>成功</remarks>
        public static string SucceesJson => Helper.Serialize(Helper.Ok);

        /// <summary>页面不存在的Json字符串</summary>
        public static string NoFindJson => Helper.Serialize(Helper.NoFind);

        /// <summary>系统不支持的Json字符串</summary>
        public static string NotSupportJson => Helper.Serialize(Helper.NonSupport);

        /// <summary>参数错误字符串</summary>
        public static string ArgumentErrorJson => Helper.Serialize(Helper.ArgumentError);

        /// <summary>逻辑错误字符串</summary>
        public static string BusinessErrorJson => Helper.Serialize(Helper.BusinessError);

        /// <summary>拒绝访问的Json字符串</summary>
        public static string DenyAccessJson => Helper.Serialize(Helper.DenyAccess);


        /// <summary>服务器访问异常</summary>
        public static string NetworkErrorJson => Helper.Serialize(Helper.NetworkError);

        /// <summary>本地访问异常的Json字符串</summary>
        public static string BusinessExceptionJson => Helper.Serialize(Helper.BusinessException);

        /// <summary>系统未就绪的Json字符串</summary>
        public static string NoReadyJson => Helper.Serialize(Helper.NoReady);

        /// <summary>暂停服务的Json字符串</summary>
        public static string PauseJson => Helper.Serialize(Helper.Pause);

        /// <summary>未知错误的Json字符串</summary>
        public static string UnknowErrorJson => Helper.Serialize(Helper.BusinessError);

        /// <summary>网络超时的Json字符串</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static string NetTimeOutJson => Helper.Serialize(Helper.NetTimeOut);

        /// <summary>令牌超时的Json字符串</summary>
        /// <remarks>令牌超时</remarks>
        public static string TokenTimeOutJson => Helper.Serialize(Helper.TokenTimeOut);

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        public static string ExecTimeOut => Helper.Serialize(Helper.ExecTimeOut);

        /// <summary>内部错误的Json字符串</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static string InnerErrorJson => Helper.Serialize(Helper.BusinessException);

        /// <summary>服务不可用的Json字符串</summary>
        public static string UnavailableJson => Helper.Serialize(Helper.Unavailable);

        #endregion

        #region 构造方法

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult Succees() => Helper.Succees();

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <returns></returns>
        public static IApiResult State(int code) => Helper.State(code);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message) => Helper.State(code, message);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message, string innerMessage) => Helper.State(code, message, innerMessage);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message, string innerMessage, string guide, string describe)
             => Helper.State(code, message, innerMessage, guide, describe);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message, string innerMessage, string point, string guide, string describe)
             => Helper.State(code, message, innerMessage, point, guide, describe);

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> Succees<TData>(TData data) => Helper.Succees<TData>(data);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code) => Helper.State<TData>(code);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message) => Helper.State<TData>(code, message);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message, string innerMessage) => Helper.State<TData>(code, message, innerMessage);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message, string innerMessage, string guide, string describe)
             => Helper.State<TData>(code, message, innerMessage, guide, describe);

        /// <summary>
        ///     生成一个包含状态码的标准返回
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="message">提示消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message, string innerMessage, string point, string guide, string describe)
             => Helper.State<TData>(code, message, point, innerMessage, guide, describe);


        #endregion

        #region 静态方法

        /// <summary>
        ///     取出上下文中的返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> FromContext<TData>()
        {
            return Helper.State<TData>(GlobalContext.Current.Status.LastStatus.Code,
                GlobalContext.Current.Status.LastStatus.Message);
        }

        /// <summary>
        ///     取出上下文中的返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult FromContext()
        {
            return Helper.State(GlobalContext.Current.Status.LastStatus.Code,
                GlobalContext.Current.Status.LastStatus.Message);
        }

        #endregion
    }
}

/*


        #region 预定义

        /// <summary>
        ///     成功
        /// </summary>
        /// <remarks>成功</remarks>
        public static ApiResult Ok => Succees();

        /// <summary>
        ///     页面不存在
        /// </summary>
        public static ApiResult NoFind => Error(ErrorCode.NoFind, "*页面不存在*");

        /// <summary>
        ///     不支持的操作
        /// </summary>
        public static ApiResult NonSupport => Error(ErrorCode.NoFind, "*页面不存在*");

        /// <summary>
        ///     参数错误字符串
        /// </summary>
        public static ApiResult ArgumentError => Error(ErrorCode.ArgumentError, "参数错误");

        /// <summary>
        ///     逻辑错误字符串
        /// </summary>
        public static ApiResult LogicalError => Error(ErrorCode.LogicalError, "逻辑错误");

        /// <summary>
        ///     拒绝访问
        /// </summary>
        public static ApiResult DenyAccess => Error(ErrorCode.DenyAccess);

        /// <summary>
        ///     服务器无返回值的字符串
        /// </summary>
        public static ApiResult RemoteEmptyError => Error(ErrorCode.RemoteError, "*服务器无返回值*");

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static ApiResult NetworkError => Error(ErrorCode.NetworkError);

        /// <summary>
        ///     本地错误
        /// </summary>
        public static ApiResult LocalError => Error(ErrorCode.LocalError);

        /// <summary>
        ///     本地访问异常
        /// </summary>
        public static ApiResult LocalException => Error(ErrorCode.LocalException);

        /// <summary>
        ///     系统未就绪
        /// </summary>
        public static ApiResult NoReady => Error(ErrorCode.NoReady);

        /// <summary>
        ///     暂停服务
        /// </summary>
        public static ApiResult Pause => Error(ErrorCode.NoReady, "暂停服务");

        /// <summary>
        ///     未知错误
        /// </summary>
        public static ApiResult UnknowError => Error(ErrorCode.LocalError, "未知错误");

        /// <summary>
        ///     网络超时
        /// </summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static ApiResult TimeOut => Error(ErrorCode.NetworkError, "网络超时");

        /// <summary>
        ///     内部错误
        /// </summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static ApiResult InnerError => Error(ErrorCode.LocalError, "内部错误");

        /// <summary>
        ///     服务不可用
        /// </summary>
        public static ApiResult Unavailable => Error(ErrorCode.Unavailable, "服务不可用");


        #endregion

        #region JSON

        /// <summary>
        ///     成功的Json字符串
        /// </summary>
        /// <remarks>成功</remarks>
        public static string SucceesJson => JsonConvert.SerializeObject(Ok);

        /// <summary>
        ///     页面不存在的Json字符串
        /// </summary>
        public static string NoFindJson => JsonConvert.SerializeObject(NoFind);

        /// <summary>
        ///     系统不支持的Json字符串
        /// </summary>
        public static string NotSupportJson => JsonConvert.SerializeObject(NonSupport);

        /// <summary>
        ///     参数错误字符串
        /// </summary>
        public static string ArgumentErrorJson => JsonConvert.SerializeObject(ArgumentError);

        /// <summary>
        ///     逻辑错误字符串
        /// </summary>
        public static string LogicalErrorJson => JsonConvert.SerializeObject(LogicalError);

        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        public static string DenyAccessJson => JsonConvert.SerializeObject(DenyAccess);

        /// <summary>
        ///     服务器无返回值的字符串
        /// </summary>
        public static string RemoteEmptyErrorJson => JsonConvert.SerializeObject(RemoteEmptyError);

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static string NetworkErrorJson => JsonConvert.SerializeObject(NetworkError);

        /// <summary>
        ///     本地错误
        /// </summary>
        public static string LocalErrorJson => JsonConvert.SerializeObject(LocalError);

        /// <summary>
        ///     本地访问异常的Json字符串
        /// </summary>
        public static string LocalExceptionJson => JsonConvert.SerializeObject(LocalException);

        /// <summary>
        ///     系统未就绪的Json字符串
        /// </summary>
        public static string NoReadyJson => JsonConvert.SerializeObject(NoReady);

        /// <summary>
        ///     暂停服务的Json字符串
        /// </summary>
        public static string PauseJson => JsonConvert.SerializeObject(Pause);

        /// <summary>
        ///     未知错误的Json字符串
        /// </summary>
        public static string UnknowErrorJson => JsonConvert.SerializeObject(UnknowError);

        /// <summary>
        ///     网络超时的Json字符串
        /// </summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static string TimeOutJson => JsonConvert.SerializeObject(TimeOut);

        /// <summary>
        ///     内部错误的Json字符串
        /// </summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static string InnerErrorJson => JsonConvert.SerializeObject(InnerError);

        /// <summary>
        ///     服务不可用的Json字符串
        /// </summary>
        public static string UnavailableJson => JsonConvert.SerializeObject(Unavailable);


        #endregion
*/
