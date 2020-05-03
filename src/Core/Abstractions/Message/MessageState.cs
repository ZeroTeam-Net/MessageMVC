using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息状态
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// 未消费
        /// </summary>
        None = 0,

        /// <summary>
        /// 取消处理
        /// </summary>
        Cancel = 1,

        /// <summary>
        /// 不支持处理
        /// </summary>
        NonSupport = 3,

        /// <summary>
        /// 已接受
        /// </summary>
        Accept = 0x10,

        /// <summary>
        /// 未发送
        /// </summary>
        Unsend = 0x11,

        /// <summary>
        /// 已发送
        /// </summary>
        Send = 0x12,

        /// <summary>
        /// 已接收
        /// </summary>
        Recive = 0x13,

        /// <summary>
        /// 正在处理
        /// </summary>
        Processing = 0x1F,

        /// <summary>
        /// 异步排队
        /// </summary>
        AsyncQueue = 0x20,

        /// <summary>
        /// 处理成功
        /// </summary>
        Success = 0x21,

        /// <summary>
        /// 处理失败
        /// </summary>
        Failed = 0x22,

        /// <summary>
        /// 无处理结果
        /// </summary>
        Unhandled = 0x23,

        /// <summary>
        /// 拒绝处理
        /// </summary>
        Deny = 0x24,

        /// <summary>
        /// 格式错误
        /// </summary>
        FormalError = 0x25,

        /// <summary>
        /// 网络错误
        /// </summary>
        NetworkError = 0x26,

        /// <summary>
        /// 处理错误
        /// </summary>
        BusinessError = 0x27,

        /// <summary>
        /// 并非MessageMVC服务
        /// </summary>
        NoUs = 0x28,

        /// <summary>
        /// 框架错误
        /// </summary>
        FrameworkError = 0x2F,

    }
    /// <summary>
    /// 扩展
    /// </summary>
    public static class MessageStateHelper
    {
        /// <summary>
        /// 转为标准错误码
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static int ToErrorCode(this MessageState state)
        {
            switch (state)
            {
                case MessageState.None:
                    return OperatorStatusCode.Unknow;
                case MessageState.Cancel:
                    return OperatorStatusCode.Unavailable;
                case MessageState.NonSupport:
                    return OperatorStatusCode.Ignore;
                case MessageState.Accept:
                case MessageState.Send:
                case MessageState.Processing:
                case MessageState.AsyncQueue:
                    return OperatorStatusCode.Queue;
                case MessageState.Success:
                    return OperatorStatusCode.Success;
                case MessageState.Failed:
                    return OperatorStatusCode.BusinessError;
                case MessageState.Unhandled:
                    return OperatorStatusCode.NoFind;
                case MessageState.Deny:
                    return OperatorStatusCode.DenyAccess;
                case MessageState.FormalError:
                    return OperatorStatusCode.ArgumentError;
                case MessageState.NetworkError:
                    return OperatorStatusCode.NetworkError;
                case MessageState.BusinessError:
                    return OperatorStatusCode.BusinessError;
                case MessageState.FrameworkError:
                    return OperatorStatusCode.UnhandleException;
                default:
                    return OperatorStatusCode.Unknow;
            }
        }


        /// <summary>
        /// 是否完成操作
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsComplete(this MessageState state)
        {
            return (state == MessageState.Cancel || state > MessageState.AsyncQueue);
        }

        /// <summary>
        /// 是否无错误结束(成功失败或进入队列)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsEnd(this MessageState state)
        {
            switch (state)
            {
                case MessageState.AsyncQueue:
                case MessageState.Success:
                case MessageState.Failed:
                case MessageState.FormalError:
                case MessageState.Deny:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 是否无错误结束(成功失败或进入队列)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsSuccess(this MessageState state)
        {
            switch (state)
            {
                case MessageState.AsyncQueue:
                case MessageState.Success:
                    return true;
                default:
                    return false;
            }
        }
    }
}
