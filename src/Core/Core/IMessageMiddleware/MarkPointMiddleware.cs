using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 埋点发出中间件
    /// </summary>
    public class MarkPointMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task> next)
        {
            await next();
            _ = MessagePoster.PublishAsync(ZeroFlowControl.Config.MarkPointName, message.Topic, message);
        }
    }
}