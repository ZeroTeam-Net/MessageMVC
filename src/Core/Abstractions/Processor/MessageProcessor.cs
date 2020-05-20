using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Services;

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
        /// 是否离线调用
        /// </summary>
        private bool IsOffline;

        /// <summary>
        /// 调用的原始内容
        /// </summary>
        internal object Original;

        /// <summary>
        /// 消息处理(异步)
        /// </summary>
        /// <param name="service">服务</param>
        /// <param name="message">消息</param>
        /// <param name="offline">是否离线消息</param>
        /// <param name="original">原始透传对象</param>
        public static Task OnMessagePush(IService service, IInlineMessage message, bool offline, object original)
        {
            message.RealState = MessageState.Recive;
            var process = new MessageProcessor
            {
                Service = service,
                Message = message,
                Original = original,
                IsOffline = offline,
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

        /// <summary>
        /// 活动实例
        /// </summary>
        public static readonly AsyncLocal<ScopeData> Local = new AsyncLocal<ScopeData>();

        private async Task Process()
        {
            await Task.Yield();
            using (DependencyScope.CreateScope($"[MessageProcessor.Process] {Message.Topic}/{Message.Title}"))
            {
                try
                {
                    middlewares = DependencyHelper.ServiceProvider.GetServices<IMessageMiddleware>().OrderBy(p => p.Level).ToArray();

                    //BUG:Prepare处理需要检查
                    if (await Prepare() && Message.State <= MessageState.Processing)
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
            LogRecorder.BeginStepMonitor("[MessageProcessor.Prepare]");

            LogRecorder.BeginStepMonitor("[Message.Prepare]");
            Message.Reset();
            if (IsOffline)
            {
                Message.DataState = MessageDataState.ArgumentOffline;
            }
            await Message.PrepareInline();
            LogRecorder.EndStepMonitor();

            try
            {
                var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Prepare)).ToArray();
                foreach (var middleware in array)
                {
                    LogRecorder.BeginStepMonitor(middleware.GetTypeName());

                    try
                    {
                        LogRecorder.MonitorDetails(() => $"[MessageProcessor.Prepare>{middleware.GetTypeName()}.Prepare]");
                        if (!await middleware.Prepare(Service, Message, Original))
                            return false;
                    }
                    catch (Exception ex)
                    {
                        LogRecorder.MonitorInfomation(() => $"[MessageProcessor.Prepare>{middleware.GetTypeName()}.Prepare]  发生异常.{ ex.Message}.");
                        LogRecorder.Exception(ex);
                        Message.RealState = MessageState.FrameworkError;
                        return false;
                    }
                    finally
                    {
                        LogRecorder.EndStepMonitor();
                    }
                }
                if (GlobalContext.CurrentNoLazy == null)
                {
                    GlobalContext.Current.Message = Message;
                    GlobalContext.Current.Trace = Message.Trace;
                }
                return true;
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
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
            catch (Exception ex)
            {
                await OnMessageError(ex);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
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
                LogRecorder.BeginStepMonitor(next.GetTypeName());
                await next.Handle(Service, Message, Original, Handle);
                LogRecorder.EndStepMonitor();
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
            LogRecorder.BeginStepMonitor("[MessageProcessor.Write]");
            Message.Trace ??= GlobalContext.CurrentNoLazy?.Trace;
            if (Message.Trace != null)
                Message.Trace.End = DateTime.Now;
            try
            {
                if (Original is TaskCompletionSource<IMessageResult> task)//内部自调用,无需处理
                {
                    LogRecorder.MonitorDetails(() => $"[MessageProcessor.Write] 内部调用,直接返回Task");
                    task.TrySetResult(null);//本地直接使用消息
                }
                else
                {
                    LogRecorder.MonitorDetails(() => $"[MessageProcessor.Write>{Service.Receiver.GetTypeName()}.OnResult] 写入返回值");
                    await Service.Receiver.OnResult(Message, Original);
                }
            }
            catch (Exception ex)
            {
                LogRecorder.MonitorInfomation(() => $"[MessageProcessor.Write>{Service.Receiver.GetTypeName()}.OnResult] 发生异常.{ ex.Message}.");
                LogRecorder.Exception(ex);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
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
            LogRecorder.Exception(ex);
            LogRecorder.MonitorInfomation(() => $"发生未处理异常.[{Message.State}]{Message.Result}");

            Message.RealState = MessageState.FrameworkError;
            var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Exception)).ToArray();
            if (array.Length != 0)
            {
                return;
            }
            LogRecorder.BeginStepMonitor("[MessageProcessor.OnMessageError]");
            foreach (var middleware in array)
            {
                try
                {
                    LogRecorder.MonitorDetails(() => $"[{middleware.GetTypeName()}.OnGlobalException]");
                    await middleware.OnGlobalException(Service, Message, ex, Original);
                }
                catch (Exception e)
                {
                    LogRecorder.MonitorInfomation(() => $"[{middleware.GetTypeName()}.OnGlobalException] 发生异常.{ ex.Message}.");
                    LogRecorder.Exception(e);
                }
            }
            LogRecorder.EndStepMonitor();
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
            LogRecorder.BeginStepMonitor("[MessageProcessor.OnEnd]");
            Message.Offline(null);
            foreach (var middleware in array.OrderByDescending(p => p.Level))
            {
                try
                {
                    LogRecorder.BeginStepMonitor($"[{middleware.GetTypeName()}.OnEnd]");
                    await middleware.OnEnd(Message);
                }
                catch (Exception ex)
                {
                    LogRecorder.MonitorInfomation(() => $"[{middleware.GetTypeName()}.OnEnd] 发生异常.{ ex.Message}.");
                    LogRecorder.Exception(ex);
                }
                finally
                {
                    LogRecorder.EndStepMonitor();
                }
            }
            LogRecorder.EndStepMonitor();
        }
        #endregion
    }
}