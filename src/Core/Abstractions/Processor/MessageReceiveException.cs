using Agebull.Common.Logging;
using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息业务处理过程的异常,用于集中处理,也基于透明化消息接收过程的需要
    /// </summary>
    public class MessageBusinessException : Exception
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exception">原始异常</param>
        public MessageBusinessException(string msg, Exception exception) : base(msg, exception)
        {
            
        }
    }
}