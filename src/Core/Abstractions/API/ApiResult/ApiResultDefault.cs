using Agebull.Common;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API返回基类</summary>
    public class ApiResultDefault : IApiResultDefault
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IApiResult DeserializeObject(string json)
        {
            return JsonHelper.DeserializeObject<ApiResult>(json);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IApiResult<T> DeserializeObject<T>(string json)
        {
            return JsonHelper.DeserializeObject<ApiResult<T>>(json);
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public IApiResult Error(int errCode)
        {
            return new ApiResult
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message)
        {
            return ErrorBuilder(errCode, message);
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public static IApiResult ErrorBuilder(int errCode, string message = null)
        {
            return new ApiResult
            {
                Success = errCode == 0,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }
        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message, string innerMessage)
        {
            return new ApiResult
            {
                Success = errCode == 0,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == 0,
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

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == 0,
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

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        public IApiResult<TData> Succees<TData>(TData data)
        {
            return new ApiResult<TData>
            {
                Success = true,
                ResultData = data
            };
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiResult<TData>
            {
                Success = errCode == 0,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == 0,
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

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == 0,
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

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        public IApiResult Error()
        {
            return ApiResultHelper.Error();
        }

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>()
        {
            return ApiResultHelper.Error<TData>();
        }

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        public IApiResult Succees()
        {
            return ApiResultHelper.Succees();
        }

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        public IApiResult<TData> Succees<TData>()
        {
            return ApiResultHelper.Succees<TData>();
        }

        /// <summary>成功</summary>
        /// <remarks>成功</remarks>
        public IApiResult Ok => ErrorBuilder(DefaultErrorCode.Success);

        /// <summary>页面不存在</summary>
        public IApiResult NoFind => ErrorBuilder(DefaultErrorCode.NoFind, "*页面不存在*");

        /// <summary>不支持的操作</summary>
        public IApiResult NotSupport => ErrorBuilder(DefaultErrorCode.NoFind, "*页面不存在*");

        /// <summary>参数错误字符串</summary>
        public IApiResult ArgumentError => ErrorBuilder(DefaultErrorCode.ArgumentError, "参数错误");

        /// <summary>逻辑错误字符串</summary>
        public IApiResult LogicalError => ErrorBuilder(DefaultErrorCode.LogicalError, "逻辑错误");

        /// <summary>拒绝访问</summary>
        public IApiResult DenyAccess => ErrorBuilder(DefaultErrorCode.DenyAccess);

        /// <summary>服务器无返回值的字符串</summary>
        public IApiResult RemoteEmptyError => ErrorBuilder(DefaultErrorCode.RemoteError, "*服务器无返回值*");

        /// <summary>服务器访问异常</summary>
        public IApiResult NetworkError => ErrorBuilder(DefaultErrorCode.NetworkError);

        /// <summary>本地错误</summary>
        public IApiResult LocalError => ErrorBuilder(DefaultErrorCode.LocalError);

        /// <summary>本地访问异常</summary>
        public IApiResult LocalException => ErrorBuilder(DefaultErrorCode.LocalException);

        /// <summary>系统未就绪</summary>
        public IApiResult NoReady => ErrorBuilder(DefaultErrorCode.NoReady);

        /// <summary>暂停服务</summary>
        public IApiResult Pause => ErrorBuilder(DefaultErrorCode.NoReady, "暂停服务");

        /// <summary>未知错误</summary>
        public IApiResult UnknowError => ErrorBuilder(DefaultErrorCode.LocalError, "未知错误");

        /// <summary>网络超时</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public IApiResult NetTimeOut => ErrorBuilder(DefaultErrorCode.NetworkError, "网络超时");

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        public IApiResult ExecTimeOut => ErrorBuilder(DefaultErrorCode.RemoteError, "执行超时");

        /// <summary>内部错误</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public IApiResult InnerError => ErrorBuilder(DefaultErrorCode.LocalError, "内部错误");

        /// <summary>服务不可用</summary>
        public IApiResult Unavailable => ErrorBuilder(DefaultErrorCode.Unavailable, "服务不可用");
    }
}