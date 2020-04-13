using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API返回基类</summary>
    internal class ApiResultDefault : IApiResultHelper
    {
        #region 序列化

        static ISerializeProxy Serializer = DependencyHelper.Create<ISerializeProxy>();

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public string Serialize<T>(T t)
        {
            return Serializer.ToString(t);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult IApiResultHelper.Deserialize(string str)
        {
            return Serializer.ToObject<ApiResult>(str);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public IApiResult<T> Deserialize<T>(string str)
        {
            return Serializer.ToObject<ApiResult<T>>(str);
        }

        /// <summary>
        /// 反序列化(BUG:interface构造)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IApiResult<T> DeserializeInterface<T>(string json)
        {
            return Serializer.ToObject<ApiResult<T>>(json);
        }

        #endregion

        #region 基本定义

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public IApiResult Generate(int errCode, string message = null)
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
        IApiResult IApiResultHelper.Ok => Generate(DefaultErrorCode.Success);

        /// <summary>页面不存在</summary>
        IApiResult IApiResultHelper.NoFind => Generate(DefaultErrorCode.NoFind, "*页面不存在*");

        /// <summary>不支持的操作</summary>
        IApiResult IApiResultHelper.NotSupport => Generate(DefaultErrorCode.Ignore, "*不支持的操作*");

        /// <summary>参数错误字符串</summary>
        IApiResult IApiResultHelper.ArgumentError => Generate(DefaultErrorCode.ArgumentError, "参数错误");

        /// <summary>拒绝访问</summary>
        IApiResult IApiResultHelper.DenyAccess => Generate(DefaultErrorCode.DenyAccess);

        /// <summary>系统未就绪</summary>
        IApiResult IApiResultHelper.NoReady => Generate(DefaultErrorCode.NoReady);

        /// <summary>暂停服务</summary>
        IApiResult IApiResultHelper.Pause => Generate(DefaultErrorCode.NoReady, "暂停服务");

        /// <summary>逻辑错误</summary>
        IApiResult IApiResultHelper.BusinessError => Generate(DefaultErrorCode.BusinessError, "逻辑错误");
        /// <summary>
        /// 服务异常
        /// </summary>
        IApiResult IApiResultHelper.BusinessException => Generate(DefaultErrorCode.BusinessException, "服务异常");

        IApiResult IApiResultHelper.UnhandleException => Generate(DefaultErrorCode.UnhandleException, "系统异常");

        /// <summary>网络错误</summary>
        IApiResult IApiResultHelper.NetworkError => Generate(DefaultErrorCode.NetworkError);

        /// <summary>网络超时</summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        IApiResult IApiResultHelper.NetTimeOut => Generate(DefaultErrorCode.NetworkError, "网络超时");

        /// <summary>执行超时</summary>
        /// <remarks>Api执行超时</remarks>
        IApiResult IApiResultHelper.ExecTimeOut => Generate(DefaultErrorCode.TimeOut, "执行超时");

        /// <summary>服务不可用</summary>
        IApiResult IApiResultHelper.Unavailable => Generate(DefaultErrorCode.Unavailable, "服务不可用");

        #endregion

        #region 构造方法

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        IApiResult IApiResultHelper.Succees()
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
        IApiResult IApiResultHelper.Error(int errCode)
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
        IApiResult IApiResultHelper.Error(int errCode, string message)
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
        IApiResult IApiResultHelper.Error(int errCode, string message, string innerMessage)
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
        IApiResult IApiResultHelper.Error(int errCode, string message, string innerMessage, string guide, string describe)
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
        IApiResult IApiResultHelper.Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
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
        public IApiResult<TData> Succees<TData>(TData data)
        {
            return new ApiResult<TData>
            {
                Success = true,
                ResultData = data
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
        IApiResult IApiResultHelper.Error()
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
