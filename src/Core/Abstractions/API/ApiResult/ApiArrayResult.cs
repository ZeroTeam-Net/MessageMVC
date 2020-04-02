using Newtonsoft.Json;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API返回数组泛型类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiArrayResult<TData> : ApiResult, IApiResult<List<TData>>
    {
        /// <summary>
        ///     返回列表
        /// </summary>
        public List<TData> Data => ResultData;

        /// <summary>
        ///     返回列表
        /// </summary>
        [JsonProperty("data")]
        public List<TData> ResultData { get; set; }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static new ApiArrayResult<TData> Error(int errCode)
        {
            return new ApiArrayResult<TData>
            {
                Success = false,
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = ErrorCode.GetMessage(errCode)
                }
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        public static new ApiArrayResult<TData> Error(int errCode, string message)
        {
            return new ApiArrayResult<TData>
            {
                Success = errCode == ErrorCode.Success,
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode)
                }
            };
        }

        /// <summary>
        ///     生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        public static new ApiArrayResult<TData> Error(int errCode, string message, string innerMessage)
        {
            return new ApiArrayResult<TData>
            {
                Success = errCode == ErrorCode.Success,
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage
                }
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
        public static new ApiArrayResult<TData> Error(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiArrayResult<TData>
            {
                Success = errCode == ErrorCode.Success,
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage,
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
        public static new ApiArrayResult<TData> Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiArrayResult<TData>
            {
                Success = errCode == ErrorCode.Success,
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage,
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
        public static ApiArrayResult<TData> Succees(List<TData> data, string message = null)
        {
            return message == null
                ? new ApiArrayResult<TData>
                {
                    Success = true,
                    ResultData = data
                }
                : new ApiArrayResult<TData>
                {
                    Success = true,
                    ResultData = data,
                    Status = new OperatorStatus
                    {
                        Message = message
                    }
                };
        }

        /// <summary>
        ///     生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static new ApiArrayResult<TData> Error()
        {
            return new ApiArrayResult<TData>
            {
                Success = false,
                Status =GlobalContext.CurrentNoLazy?.LastStatus
            };
        }

    }
}