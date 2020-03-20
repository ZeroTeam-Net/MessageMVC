using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace KafkaTest
{
    /// <summary>
    /// 消息处理中间件
    /// </summary>
    public class MessageMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        public async Task<MessageState> Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            await next();
            Console.WriteLine("MessageMiddleware 2");
            return MessageState.Accept;
        }
    }
}
