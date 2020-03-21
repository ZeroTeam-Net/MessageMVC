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
        /// 层级
        /// </summary>
        int Level { get; }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        Task<MessageState> Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next);
    }
}