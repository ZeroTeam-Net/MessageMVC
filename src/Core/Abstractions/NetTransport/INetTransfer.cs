using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.MessageTransfers
{
    /// <summary>
    /// 表示一个网络传输对象
    /// </summary>
    public interface INetTransfer : IMessagePoster
    {
        /// <summary>
        /// 服务
        /// </summary>
        IService Service { get; set; }

        /// <summary>
        /// 关闭
        /// </summary>
        void End()
        {
        }

        /// <summary>
        /// 准备
        /// </summary>
        bool Prepare() => true;

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        Task Close()
        {
            State = StationStateType.Closed;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 开始轮询前的工作
        /// </summary>
        /// <returns></returns>
        Task<bool> LoopBegin()
        {
            State = StationStateType.Run;
            return Task.FromResult(true);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        Task<bool> Loop(CancellationToken token);

        /// <summary>
        /// 结束轮询前的工作
        /// </summary>
        /// <returns></returns>
        Task LoopComplete() => Task.CompletedTask;

        /// <summary>
        /// 表示已成功接收 
        /// </summary>
        /// <returns></returns>
        Task Commit() => Task.CompletedTask;

        /// <summary>
        /// 返回值已给出
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        async Task<bool> OnMessageResult(MessageProcessor processor, IMessageItem message, object tag)
        {
            if (tag == null)//内部自调用,无需处理
                return true;
            return await OnResult(message, tag);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns>发送是否成功</returns>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        Task<bool> OnResult(IMessageItem message, object tag) => Task.FromResult(true);

    }
}