using Agebull.Common;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API返回基类</summary>
    public static class ApiResultHelper
    {
        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        private static IApiResultDefault _ioc;

        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        public static IApiResultDefault Ioc =>
            _ioc ?? (_ioc = IocHelper.Create<IApiResultDefault>() ?? new ApiResultDefault());



        /// <summary>成功的Json字符串</summary>
        /// <remarks>成功</remarks>
        public static string SucceesJson => JsonHelper.SerializeObject(Ioc.Ok);

        /// <summary>页面不存在的Json字符串</summary>
        public static string NoFindJson => JsonHelper.SerializeObject(Ioc.NoFind);

        /// <summary>系统不支持的Json字符串</summary>
        public static string NotSupportJson => JsonHelper.SerializeObject(Ioc.NotSupport);

        /// <summary>参数错误字符串</summary>
        public static string ArgumentErrorJson => JsonHelper.SerializeObject(Ioc.ArgumentError);

        /// <summary>逻辑错误字符串</summary>
        public static string LogicalErrorJson => JsonHelper.SerializeObject(Ioc.LogicalError);

        /// <summary>拒绝访问的Json字符串</summary>
        public static string DenyAccessJson => JsonHelper.SerializeObject(Ioc.DenyAccess);

        /// <summary>服务器无返回值的字符串</summary>
        public static string RemoteEmptyErrorJson => JsonHelper.SerializeObject(Ioc.RemoteEmptyError);

        /// <summary>服务器访问异常</summary>
        public static string NetworkErrorJson => JsonHelper.SerializeObject(Ioc.NetworkError);

        /// <summary>本地错误</summary>
        public static string LocalErrorJson => JsonHelper.SerializeObject(Ioc.LocalError);

        /// <summary>本地访问异常的Json字符串</summary>
        public static string LocalExceptionJson => JsonHelper.SerializeObject(Ioc.LocalException);

        /// <summary>系统未就绪的Json字符串</summary>
        public static string NoReadyJson => JsonHelper.SerializeObject(Ioc.NoReady);

        /// <summary>暂停服务的Json字符串</summary>
        public static string PauseJson => JsonHelper.SerializeObject(Ioc.Pause);

        /// <summary>未知错误的Json字符串</summary>
        public static string UnknowErrorJson => JsonHelper.SerializeObject(Ioc.UnknowError);

        /// <summary>网络超时的Json字符串</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static string TimeOutJson => JsonHelper.SerializeObject(Ioc.NetTimeOut);

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        public static string ExecTimeOut => JsonHelper.SerializeObject(Ioc.ExecTimeOut);

        /// <summary>内部错误的Json字符串</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static string InnerErrorJson => JsonHelper.SerializeObject(Ioc.InnerError);

        /// <summary>服务不可用的Json字符串</summary>
        public static string UnavailableJson => JsonHelper.SerializeObject(Ioc.Unavailable);

        #region 构造方法

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode)
        {
            return new ApiResult
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }
        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Point = point,
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Succees<TData>(TData data, string message = null)
        {
            return new ApiResult<TData>
            {
                Success = true,
                ResultData = data,
                Message = message
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiResult<TData>
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Point = point,
                    Guide = guide,
                    Describe = describe
                }
            };
        }
        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult Error()
        {
            var result = new ApiResult
            {
                Success = false
            };
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>()
        {
            var result = new ApiResult<TData>
            {
                Success = false
            };
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }
        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult Succees()
        {
            var result = new ApiResult();
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Succees<TData>()
        {
            var result = new ApiResult<TData>();
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
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
        public static ApiResult NotSupport => Error(ErrorCode.NoFind, "*页面不存在*");

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
        public static string NotSupportJson => JsonConvert.SerializeObject(NotSupport);

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
