using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息业务处理过程的参数异常用于精确定位于框架中发的参数错误,用于集中处理,也基于透明化消息接收过程的需要
    /// </summary>
    public class MessageArgumentNullException : ArgumentNullException
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name">参数名称</param>
        public MessageArgumentNullException(string name) : base(name)
        {

        }
    }
}