﻿using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示一个消息接收对象
    /// </summary>
    public interface IMessageReceiver : IMessagePoster
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        ILogger Logger { get; set; }

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
        /// 发送返回值 
        /// </summary>
        /// <returns>发送是否成功</returns>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        Task<bool> OnResult(IInlineMessage message, object tag) => Task.FromResult(true);

    }
}