using System;

namespace ZeroTeam.MessageMVC.MessageTransfers
{
    /// <summary>
    /// 表示网络传输的异常,用于集中处理,也基于透明化网络传输对象的需要
    /// </summary>
    public class NetTransferException : Exception
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="exception">原始异常</param>
        public NetTransferException(string msg,Exception exception) : base(msg, exception)
        {

        }
    }
}