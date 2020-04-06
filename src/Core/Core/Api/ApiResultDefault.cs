using Agebull.Common;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API返回基类</summary>
    internal class ApiResultDefault : IApiResultDefault
    {
        #region 序列化

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public string SerializeObject<T>(T t)
        {
            return JsonHelper.SerializeObject(t);
        }

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
        #endregion

        #region 基本定义

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public IApiResult Build(int errCode, string message = null)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>成功</summary>
        /// <remarks>成功</remarks>
        public IApiResult Ok => Build(DefaultErrorCode.Success);

        /// <summary>页面不存在</summary>
        public IApiResult NoFind => Build(DefaultErrorCode.NoFind, "*页面不存在*");

        /// <summary>不支持的操作</summary>
        public IApiResult NotSupport => Build(DefaultErrorCode.NoFind, "*页面不存在*");

        /// <summary>参数错误字符串</summary>
        public IApiResult ArgumentError => Build(DefaultErrorCode.ArgumentError, "参数错误");

        /// <summary>逻辑错误字符串</summary>
        public IApiResult LogicalError => Build(DefaultErrorCode.LogicalError, "逻辑错误");

        /// <summary>拒绝访问</summary>
        public IApiResult DenyAccess => Build(DefaultErrorCode.DenyAccess);

        /// <summary>服务器无返回值的字符串</summary>
        public IApiResult RemoteEmptyError => Build(DefaultErrorCode.RemoteError, "*服务器无返回值*");

        /// <summary>服务器访问异常</summary>
        public IApiResult NetworkError => Build(DefaultErrorCode.NetworkError);

        /// <summary>本地错误</summary>
        public IApiResult LocalError => Build(DefaultErrorCode.LocalError);

        /// <summary>本地访问异常</summary>
        public IApiResult LocalException => Build(DefaultErrorCode.LocalException);

        /// <summary>系统未就绪</summary>
        public IApiResult NoReady => Build(DefaultErrorCode.NoReady);

        /// <summary>暂停服务</summary>
        public IApiResult Pause => Build(DefaultErrorCode.NoReady, "暂停服务");

        /// <summary>未知错误</summary>
        public IApiResult UnknowError => Build(DefaultErrorCode.LocalError, "未知错误");

        /// <summary>网络超时</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public IApiResult NetTimeOut => Build(DefaultErrorCode.NetworkError, "网络超时");

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        public IApiResult ExecTimeOut => Build(DefaultErrorCode.RemoteError, "执行超时");

        /// <summary>内部错误</summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public IApiResult InnerError => Build(DefaultErrorCode.LocalError, "内部错误");

        /// <summary>服务不可用</summary>
        public IApiResult Unavailable => Build(DefaultErrorCode.Unavailable, "服务不可用");

        #endregion

        #region 构造方法

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public IApiResult Succees()
        {
            return new ApiResult
            {
                Success = true
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
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

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message)
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
        public IApiResult Error(int errCode, string message, string innerMessage)
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
        public IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
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
        public IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
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
        public IApiResult<TData> Succees<TData>(TData data, string message = null)
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
        public IApiResult<TData> Error<TData>(int errCode)
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
        public IApiResult<TData> Error<TData>(int errCode, string message)
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
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage)
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
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
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
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
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
        public IApiResult Error()
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
        public IApiResult<TData> Error<TData>()
        {
            var result = new ApiResult<TData>
            {
                Success = false
            };
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Success = GlobalContext.Current.Status.LastStatus.Success;
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }

        #endregion

    }
}

/*

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
            return Error(errCode, message);
        }

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public  IApiResult Error(int errCode, string message = null)
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
*/
