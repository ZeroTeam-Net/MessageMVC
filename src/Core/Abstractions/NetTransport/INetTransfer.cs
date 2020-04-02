using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个网络传输对象
    /// </summary>
    public interface INetTransfer
    {
        /// <summary>
        /// 服务
        /// </summary>
        IService Service { get; set; }


        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize()
        {
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void End()
        {
        }
        /// <summary>
        /// 将要开始
        /// </summary>
        Task<bool> Prepare() => Task.FromResult(true);

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        Task Close() => Task.CompletedTask;

        /// <summary>
        /// 开始轮询前的工作
        /// </summary>
        /// <returns></returns>
        Task<bool> LoopBegin() => Task.FromResult(true);

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
        async Task OnMessageResult(IMessageItem message, object tag)
        {
            try
            {
                await OnResult(message, tag);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
            await OnCallEnd(message, tag);
        }


        /// <summary>
        /// 错误发生时处理
        /// </summary>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        async Task OnMessageError(Exception exception, IMessageItem message, object tag)
        {
            if (exception is NetTransferException ne)
            {
                message.State = MessageState.NetError;
                message.Result = ne.InnerException.Message;
            }
            else
            {
                if (message.State <= MessageState.Accept)
                    message.State = MessageState.Exception;
                message.Result = exception.Message;
            }
            try
            {
                await OnError(exception, message, tag);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
            await OnCallEnd(message, tag);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        Task OnResult(IMessageItem message, object tag) => Task.CompletedTask;

        /// <summary>
        /// 错误 
        /// </summary>
        /// <returns></returns>
        Task OnError(Exception exception, IMessageItem message, object tag) => Task.CompletedTask;

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns></returns>
        Task OnCallEnd(IMessageItem message, object tag) => Task.CompletedTask;
    }
}