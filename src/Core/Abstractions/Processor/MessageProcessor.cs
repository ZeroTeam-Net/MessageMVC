using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    ///    消息处理器
    /// </summary>
    public class MessageProcessor
    {
        #region 处理入口
        TaskCompletionSource<IMessageItem> WaitTask;
        /// <summary>
        /// 当前站点
        /// </summary>
        internal IService Service;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal IInlineMessage Message;

        /// <summary>
        /// 调用的原始内容
        /// </summary>
        internal object Original;

        /// <summary>
        /// 消息处理(异步)
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <param name="original"></param>
        public static Task OnMessagePush(IService service, IInlineMessage message, object original)
        {
            var process = new MessageProcessor
            {
                Service = service,
                Message = message,
                Original = original,
                WaitTask = new TaskCompletionSource<IMessageItem>()
            };
            Task.Factory.StartNew(process.Process);
            return process.WaitTask.Task;
        }

        #endregion

        #region 中间件链式调用

        /// <summary>
        /// 所有消息处理中间件
        /// </summary>
        private IMessageMiddleware[] middlewares;

        /// <summary>
        /// 当前中间件序号
        /// </summary>
        private int index = 0;

        private async Task Process()
        {
            await Task.Yield();
            using (DependencyScope.CreateScope($"[MessageProcessor.Process] {Message.Topic}/{Message.Title}"))
            {
                try
                {
                    middlewares = DependencyHelper.ServiceProvider.GetServices<IMessageMiddleware>().OrderBy(p => p.Level).ToArray();
                    foreach (var middleware in middlewares)
                    {
                        middleware.Processor = this;
                    }
                    Message.Reset();
                    await Message.PrepareInline();

                    if (await Prepare() && Message.State == MessageState.None)
                    {
                        index = 0;
                        await DoHandle();
                    }

                    await Write();
                }
                finally
                {
                    WaitTask.SetResult(Message);
                }
                await OnEnd();
            }
        }

        /// <summary>
        /// 准备处理
        /// </summary>
        /// <returns>是否需要正式处理</returns>
        private async Task<bool> Prepare()
        {
            var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Prepare)).ToArray();
            if (array.Length == 0)
                return true;
            foreach (var middleware in array)
            {
                try
                {
                    LogRecorder.MonitorTrace(() => $"[MessageProcessor.Prepare>{middleware.GetTypeName()}.Prepare]");
                    if (!await middleware.Prepare(Service, Message, Original))
                        return false;
                }
                catch (Exception ex)
                {
                    LogRecorder.MonitorTrace(() => $"[MessageProcessor.Prepare>{middleware.GetTypeName()}.Prepare]  发生异常.{ ex.Message}.");
                    LogRecorder.Exception(ex);
                    Message.State = MessageState.Error;
                    return false;
                }
            }
            if (GlobalContext.CurrentNoLazy == null)
                GlobalContext.Current.Message = Message;
            return true;
        }

        /// <summary>
        /// 中间件链式处理
        /// </summary>
        /// <returns></returns>
        private async Task DoHandle()
        {
            LogRecorder.BeginStepMonitor("[MessageProcessor.DoHandle]");
            try
            {
                await Handle();
            }
            catch (OperationCanceledException ex)
            {
                Message.State = MessageState.Cancel;
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.Ignore);
                await OnMessageError(ex);
            }
            catch (ThreadInterruptedException ex)
            {
                Message.State = MessageState.Cancel;
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.Ignore);
                await OnMessageError(ex);
            }
            catch (MessageBusinessException ex)
            {
                Message.State = MessageState.Error;
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.BusinessException);
                await OnMessageError(ex);
            }
            catch (MessageReceiveException ex)
            {
                Message.State = MessageState.Error;
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.NetworkError);
                await OnMessageError(ex);
            }
            catch (Exception ex)
            {
                Message.State = MessageState.Error;
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.UnhandleException);
                await OnMessageError(ex);
            }
            LogRecorder.EndStepMonitor();
        }

        /// <summary>
        /// 中间件链式处理
        /// </summary>
        /// <returns></returns>
        private async Task Handle()
        {
            while (index < middlewares.Length)
            {
                var next = middlewares[index++];
                if (!next.Scope.HasFlag(MessageHandleScope.Handle))
                {
                    continue;
                }
                LogRecorder.MonitorTrace(() => $"[{next.GetTypeName()}.Handle]");
                await next.Handle(Service, Message, Original, Handle);
                return;
            }
        }

        #endregion

        #region 处理结果返回

        /// <summary>
        /// 结果推到调用处
        /// </summary>
        private async Task Write()
        {
            Message.ResultSerializer ??= Service.Serialize;
            Message.RuntimeStatus ??= GlobalContext.CurrentNoLazy?.Status?.LastStatus;
            Message.PrepareOffline();

            if (Original is TaskCompletionSource<IMessageResult> task)//内部自调用,无需处理
            {
                Message.Offline(Service.Serialize);
                task.TrySetResult(Message.ToMessageResult());
                LogRecorder.MonitorTrace(() => $"[MessageProcessor.Write] 内部调用,直接返回Task");
                return;
            }
            try
            {
                LogRecorder.MonitorTrace(() => $"[MessageProcessor.Write>{Service.Receiver.GetTypeName()}.OnResult] 写入返回值");
                await Service.Receiver.OnResult(Message, Original);
            }
            catch (Exception ex)
            {
                LogRecorder.MonitorTrace(() => $"[MessageProcessor.Write>{Service.Receiver.GetTypeName()}.OnResult] 发生异常.{ ex.Message}.");
                LogRecorder.Exception(ex);
            }
        }

        #endregion

        #region 异常处理

        /// <summary>
        /// 错误发生时处理
        /// </summary>
        /// <remarks>
        /// 默认实现为保证OnCallEnd可控制且不再抛出异常,无特殊需要不应再次实现
        /// </remarks>
        private async Task OnMessageError(Exception ex)
        {
            Message.RuntimeStatus.Exception = ex;
            LogRecorder.Exception(ex);
            LogRecorder.MonitorTrace(() => $"发生未处理异常.[{Message.RuntimeStatus.Code}]{Message.RuntimeStatus.Message}");

            var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Exception)).ToArray();
            if (array.Length == 0)
            {
                LogRecorder.BeginStepMonitor("[MessageProcessor.OnMessageError]");
                foreach (var middleware in array)
                {
                    try
                    {
                        LogRecorder.MonitorTrace(() => $"[{middleware.GetTypeName()}.OnGlobalException]");
                        await middleware.OnGlobalException(Service, Message, Original);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.MonitorTrace(() => $"[{middleware.GetTypeName()}.OnGlobalException] 发生异常.{ ex.Message}.");
                        LogRecorder.Exception(e);
                    }
                }
                LogRecorder.EndStepMonitor();
            }
            Message.RuntimeStatus ??= ApiResultHelper.Error(DefaultErrorCode.UnhandleException, ex.Message);
        }
        #endregion

        #region 数据发送结束

        /// <summary>
        /// 数据发送结束
        /// </summary>
        async Task OnEnd()
        {
            var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.End)).ToArray();
            if (array.Length == 0)
                return;
            LogRecorder.BeginStepMonitor("[MessageProcessor.Last]");
            Message.Offline(null);
            foreach (var middleware in array)
            {
                try
                {
                    LogRecorder.MonitorTrace(() => $"[{middleware.GetTypeName()}.OnEnd]");
                    await middleware.OnEnd(Message);
                }
                catch (Exception ex)
                {
                    LogRecorder.MonitorTrace(() => $"[{middleware.GetTypeName()}.OnEnd] 发生异常.{ ex.Message}.");
                    LogRecorder.Exception(ex);
                }
            }
            LogRecorder.EndStepMonitor();
        }
        #endregion
    }
}