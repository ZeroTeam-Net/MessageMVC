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
        async Task OnMessageResult(MessageProcessor processor, IMessageItem message, object tag)
        {
            if (tag == null)//内部自调用,无需处理
                return;
            try
            {
                await OnResult(message, tag);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
            try
            {
                await OnCallEnd(message, tag);
                return;
            }
            catch
            {
            }
            await processor.PushResult();//停止HttpCall的等待
            await MessagePoster.PostReceipt(message);
        }


        /// <summary>
        /// 错误发生时处理
        /// </summary>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        async Task OnMessageError(MessageProcessor processor, Exception exception, IMessageItem message, object tag)
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
            if (tag == null)//内部自调用,无需处理
                return;
            try
            {
                await OnError(exception, message, tag);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }

            try
            {
                if (!await OnCallEnd(message, tag))
                    return;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
            await processor.PushResult();
            await MessagePoster.PostReceipt(message);
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
        /// <returns>是否需要发送回执(默认不发送)</returns>
        Task<bool> OnCallEnd(IMessageItem message, object tag) => Task.FromResult(false);
    }
}