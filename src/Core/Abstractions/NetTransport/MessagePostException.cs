using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息发送的异常,用于集中处理,也基于透明化消息发送过程的需要
    /// </summary>
    public class MessagePostException : Exception
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exception">原始异常</param>
        public MessagePostException(string msg, Exception exception) : base(msg, exception)
        {
            //FlowTracer.Exception(exception, msg);
        }
    }
}