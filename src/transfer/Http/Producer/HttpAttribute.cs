using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     表示Http服务
    /// </summary>
    public class HttpAttribute : Attribute, IReceiverGet
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public HttpAttribute(string name)
        {
            ServiceName = name;
        }

        /// <summary>
        /// 消息节点
        /// </summary>
        public string ServiceName { get; }

        IMessageReceiver IReceiverGet.Receiver(string service)
        {
            return new HttpReceiver();
        }
    }

}
