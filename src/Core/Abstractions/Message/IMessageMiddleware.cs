using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Messages
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
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope Scope { get; }


        /// <summary>
        /// 当前处理器
        /// </summary>
        MessageProcessor Processor { get; set; }


        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task<bool> Prepare(IService service, IMessageItem message, object tag)
        {
            return Task.FromResult(true);
        }


        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        Task Handle(IService service, IMessageItem message, object tag, Func<Task> next)
        {
            return next();
        }


        /// <summary>
        /// 全局异常发生时
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task OnGlobalException(IService service, IMessageItem message, object tag)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 处理结束时(结果交付Service后)
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task OnEnd(IMessageItem message)
        {
            return Task.CompletedTask;
        }
    }
}