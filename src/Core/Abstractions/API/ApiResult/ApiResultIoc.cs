using Agebull.Common;
using Agebull.Common.Ioc;
using System.Collections.Generic;
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
        private static IApiResultDefault _ioc;

        /// <summary>
        /// ApiResult的抽象
        /// </summary>
        public static IApiResultDefault Ioc => _ioc ??= IocHelper.Create<IApiResultDefault>();

        /// <summary>成功的Json字符串</summary>
        /// <remarks>成功</remarks>
        public static string SucceesJson => Ioc.SerializeObject(Ioc.Ok);

        /// <summary>页面不存在的Json字符串</summary>
        public static string NoFindJson => Ioc.SerializeObject(Ioc.NoFind);

        /// <summary>系统不支持的Json字符串</summary>
        public static string NotSupportJson => Ioc.SerializeObject(Ioc.NotSupport);

        /// <summary>参数错误字符串</summary>
        public static string ArgumentErrorJson => Ioc.SerializeObject(Ioc.ArgumentError);

        /// <summary>逻辑错误字符串</summary>
        public static string LogicalErrorJson => Ioc.SerializeObject(Ioc.LogicalError);

        /// <summary>拒绝访问的Json字符串</summary>
        public static string DenyAccessJson => Ioc.SerializeObject(Ioc.DenyAccess);

        /// <summary>服务器无返回值的字符串</summary>
        public static string RemoteEmptyErrorJson => Ioc.SerializeObject(Ioc.RemoteEmptyError);

        /// <summary>服务器访问异常</summary>
        public static string NetworkErrorJson => Ioc.SerializeObject(Ioc.NetworkError);

        /// <summary>本地错误</summary>
        public static string LocalErrorJson => Ioc.SerializeObject(Ioc.LocalError);

        /// <summary>本地访问异常的Json字符串</summary>
        public static string LocalExceptionJson => Ioc.SerializeObject(Ioc.LocalException);

        /// <summary>系统未就绪的Json字符串</summary>
        public static string NoReadyJson => Ioc.SerializeObject(Ioc.NoReady);

        /// <summary>暂停服务的Json字符串</summary>
        public static string PauseJson => Ioc.SerializeObject(Ioc.Pause);

        /// <summary>未知错误的Json字符串</summary>
        public static string UnknowErrorJson => Ioc.SerializeObject(Ioc.UnknowError);

        /// <summary>网络超时的Json字符串</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static string TimeOutJson => Ioc.SerializeObject(Ioc.NetTimeOut);

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        public static string ExecTimeOut => Ioc.SerializeObject(Ioc.ExecTimeOut);

        /// <summary>内部错误的Json字符串</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static string InnerErrorJson => Ioc.SerializeObject(Ioc.InnerError);

        /// <summary>服务不可用的Json字符串</summary>
        public static string UnavailableJson => Ioc.SerializeObject(Ioc.Unavailable);

        #endregion

        #region 构造方法

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult Succees() => Ioc.Succees();

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode) => Ioc.Error(errCode);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode, string message) => Ioc.Error(errCode, message);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode, string message, string innerMessage) => Ioc.Error(errCode, message, innerMessage);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
             => Ioc.Error(errCode, message, innerMessage, guide, describe);

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
        public static IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
             => Ioc.Error(errCode, message, innerMessage, point, guide, describe);

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> Succees<TData>(TData data, string message = null) => Ioc.Succees<TData>(data, message);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode) => Ioc.Error<TData>(errCode);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode, string message) => Ioc.Error<TData>(errCode, message);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage) => Ioc.Error<TData>(errCode, message, innerMessage);

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
             => Ioc.Error<TData>(errCode, message, innerMessage, guide, describe);

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
        public static IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
             => Ioc.Error<TData>(errCode, message, point, innerMessage, guide, describe);


        #endregion

        #region 静态方法

        /// <summary>
        ///     取出上下文中的返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> FromContext<TData>()
        {
            return new ApiResult<TData>
            {
                Success = GlobalContext.Current.Status.LastStatus.Success,
                Code = GlobalContext.Current.Status.LastStatus.Code,
                Message = GlobalContext.Current.Status.LastStatus.Message
            };
        }

        /// <summary>
        ///     取出上下文中的返回
        /// </summary>
        /// <returns></returns>
        public static IApiResult FromContext()
        {
            return new ApiResult
            {
                Success = GlobalContext.Current.Status.LastStatus.Success,
                Code = GlobalContext.Current.Status.LastStatus.Code,
                Message = GlobalContext.Current.Status.LastStatus.Message
            };
        }

        #region Array

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArraySuccees<TData>(List<TData> data, string message = null)
        {
            return new ApiArrayResult<TData>
            {
                Success = true,
                ResultData = data,
                Message = message
            };
        }

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiArrayResult<TData> ArrayError<TData>()
        {
            var result = new ApiArrayResult<TData>();
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Success = GlobalContext.Current.Status.LastStatus.Success;
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }
        #endregion

        #region Value

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiValueResult ValueSuccees(string data, string message = null)
        {
            return new ApiValueResult
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
        public static ApiValueResult ValueError(int errCode)
        {
            return new ApiValueResult
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
        public static ApiValueResult ValueError(int errCode, string message)
        {
            return new ApiValueResult
            {
                Success = false,
                Code = errCode,
                Message = message
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <param name="innerMessage"></param>
        /// <returns></returns>
        public static ApiValueResult ValueError(int errCode, string message, string innerMessage)
        {
            return new ApiValueResult
            {
                Success = false,
                Code = errCode,
                Message = message,
                InnerMessage = innerMessage
            };
        }

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiValueResult<TData> ValueSuccees<TData>(TData data, string message = null)
        {
            return new ApiValueResult<TData>
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
        public static ApiValueResult<TData> ValueError<TData>(int errCode)
        {
            return new ApiValueResult<TData>
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
        public static ApiValueResult<TData> ValueError<TData>(int errCode, string message)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <param name="innerMessage"></param>
        /// <returns></returns>
        public static ApiValueResult<TData> ValueError<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message,
                InnerMessage = innerMessage
            };
        }

        #endregion

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
