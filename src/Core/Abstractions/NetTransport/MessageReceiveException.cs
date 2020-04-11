using Agebull.Common.Logging;
using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息接收的异常,用于集中处理,也基于透明化消息接收过程的需要
    /// </summary>
    public class MessageReceiveException : Exception
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exception">原始异常</param>
        public MessageReceiveException(string msg, Exception exception) : base(msg, exception)
        {
            LogRecorder.Exception(exception, msg);
        }
    }
}