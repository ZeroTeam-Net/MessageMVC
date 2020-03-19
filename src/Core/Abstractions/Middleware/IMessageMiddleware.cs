using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 消息处理中间件
    /// </summary>
    public interface IMessageMiddleware
    {
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        Task<MessageState> Handle(IService service, string message,Func<Task<MessageState>> next);
    }
}