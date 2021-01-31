using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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

        ILogger logger;

        TaskCompletionSource<bool> WaitTask;
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
            ZeroAppOption.Instance.BeginRequest();
            message.RealState = MessageState.Recive;
            var process = new MessageProcessor
            {
                Service = service,
                Message = message,
                Original = original,
                IsOffline = offline,
                WaitTask = new TaskCompletionSource<bool>()
            };
            ScopeRuner.RunScope($"{message.Service}/{message.Method}", process.Process, ContextInheritType.Clone);
            return process.WaitTask.Task;
        }

        /// <summary>
        /// 消息处理(异步)
        /// </summary>
        /// <param name="service">服务</param>
        /// <param name="message">消息</param>
        /// <param name="offline">是否离线消息</param>
        /// <param name="original">原始透传对象</param>
        public static void RunOnMessagePush(IService service, IInlineMessage message, bool offline, object original)
        {
            if (!ZeroAppOption.Instance.BeginRequest())
            {
                message.RealState = MessageState.Cancel;
                return;
            }
            message.RealState = MessageState.Recive;
            var process = new MessageProcessor
            {
                Service = service,
                Message = message,
                Original = original,
                IsOffline = offline
            };
            ScopeRuner.RunScope($"{message.Service}/{message.Method}", process.Process, ContextInheritType.Clone);
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
            try
            {
                await Task.Yield();
                var name = $"{Message.Service}/{Message.Method}({Message.ID})";

                FlowTracer.BeginMonitor(name);
                logger = DependencyHelper.LoggerFactory.CreateLogger(name);
                middlewares = DependencyHelper.ServiceProvider.GetServices<IMessageMiddleware>().Where(p => p.Scope > MessageHandleScope.None).OrderBy(p => p.Level).ToArray();

                //BUG:Prepare处理需要检查
                if (await Prepare() && Message.State <= MessageState.Processing)
                {
                    index = 0;
                    await DoHandle();
                }
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
            }
            await Write();

            try
            {
                WaitTask?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                logger.Error($"[WaitTask:\n{ex}");
            }
            try
            {
                var actionTask = GlobalContext.Current?.ActionTask;
                if (actionTask != null)
                {
                    await actionTask.Task;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[ActionTask]:\n{ex}");
            }

            await OnEnd();

            ZeroAppOption.Instance.EndRequest();
        }

        /// <summary>
        /// 准备处理
        /// </summary>
        /// <returns>是否需要正式处理</returns>
        private async Task<bool> Prepare()
        {
            FlowTracer.BeginStepMonitor("[Prepare]");
            try
            {
                Message.CheckRequestTraceInfo();
                Message.ResetToRequest();
                if (IsOffline)
                {
                    Message.DataState = MessageDataState.ArgumentOffline;
                }
            }
            catch (Exception ex)
            {
                await OnMessageError(ex);
                return false;
            }

            FlowTracer.MonitorTrace(() => $"[Context]    {Message.Context?.ToInnerString()}");
            FlowTracer.MonitorTrace(() => $"[User]       {Message.User?.ToInnerString()}");
            FlowTracer.MonitorTrace(() => $"[TraceInfo]  {Message.TraceInfo?.ToInnerString()}");
            FlowTracer.MonitorDetails(() => $"[Argument]   {Message.Argument ?? Message.ArgumentData?.ToInnerString()}");
            
            try
            {
                var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Prepare)).ToArray();
                foreach (var middleware in array)
                {
                    FlowTracer.BeginDebugStepMonitor(middleware.GetTypeName());

                    try
                    {
                        FlowTracer.MonitorDetails(() => $"[Prepare>{middleware.GetTypeName()}.Prepare]");
                        if (!await middleware.Prepare(Service, Message, Original))
                            return false;
                    }
                    catch (Exception ex)
                    {
                        FlowTracer.MonitorError(() => $"[Prepare>{middleware.GetTypeName()}.Prepare]  发生异常.{ ex.Message}.");
                        await OnMessageError(ex);
                        return false;
                    }
                    finally
                    {
                        FlowTracer.EndDebugStepMonitor();
                    }
                }
                if (GlobalContext.CurrentNoLazy == null)
                {
                    GlobalContext.Current.Message = Message;
                }
                return true;
            }
            finally
            {
                FlowTracer.EndStepMonitor();
            }
        }

        /// <summary>
        /// 中间件链式处理
        /// </summary>
        /// <returns></returns>
        private async Task DoHandle()
        {
            FlowTracer.BeginStepMonitor("[DoHandle]");
            try
            {
                await Handle();
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => $"[DoHandle]  发生异常.{ ex.Message}.");
                await OnMessageError(ex);
            }
            finally
            {
                FlowTracer.EndStepMonitor();
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
                FlowTracer.BeginDebugStepMonitor(next.GetTypeName());
                await next.Handle(Service, Message, Original, Handle);
                FlowTracer.EndDebugStepMonitor();
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
            if (Original == null || Message.State == MessageState.NoUs)
                return;
            FlowTracer.BeginDebugStepMonitor("[Write]");
            //Message.Trace ??= GlobalContext.CurrentNoLazy?.Trace;
            if (Message.TraceInfo != null)
                Message.TraceInfo.End = DateTime.Now;
            try
            {
                Message.OfflineResult();
            }
            catch (Exception ex)
            {
                await OnMessageError(ex);
            }
            try
            {
                if(Original is IMessageWriter writer)
                {
                    FlowTracer.MonitorDetails(() => $"[Write>{writer}.OnResult] 写入返回值");
                    await writer.OnResult(Message, null);
                }
                else
                {
                    FlowTracer.MonitorDetails(() => $"[Write>{Service.Receiver.GetTypeName()}.OnResult] 写入返回值");
                    await Service.Receiver.OnResult(Message, Original);
                }
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => $"[Write>{Service.Receiver.GetTypeName()}.OnResult] 发生异常.{ ex.Message}.");
                logger.Exception(ex);
            }
            finally
            {
                FlowTracer.EndDebugStepMonitor();
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
            logger.Exception(ex);
            FlowTracer.MonitorError(() => $"发生未处理异常.[{Message.State}]{Message.Result}");

            Message.RealState = MessageState.FrameworkError;
            var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Exception)).ToArray();
            if (array.Length == 0)
            {
                return;
            }
            foreach (var middleware in array)
            {
                try
                {
                    FlowTracer.BeginDebugStepMonitor(() => $"[{middleware.GetTypeName()}.OnGlobalException]");
                    await middleware.OnGlobalException(Service, Message, ex, Original);
                }
                catch (Exception e)
                {
                    FlowTracer.MonitorError(() => $"[{middleware.GetTypeName()}.OnGlobalException] 发生异常.{ ex.Message}.");
                    logger.Exception(e);
                }
                finally
                {
                    FlowTracer.EndDebugStepMonitor();
                }
            }
        }
        #endregion

        #region 数据发送结束

        /// <summary>
        /// 数据发送结束
        /// </summary>
        async Task OnEnd()
        {
            FlowTracer.BeginStepMonitor("[OnEnd]");
            FlowTracer.MonitorDetails($"[State] {Message.State} [Result] {Message.Result}");
            Message.TraceInfo.End = DateTime.Now;
            var array = middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.End)).ToArray();
            if (array.Length == 0)
                return;
            foreach (var middleware in array.OrderByDescending(p => p.Level))
            {
                try
                {
                    FlowTracer.BeginDebugStepMonitor($"[{middleware.GetTypeName()}.OnEnd]");
                    await middleware.OnEnd(Message);
                }
                catch (Exception ex)
                {
                    FlowTracer.MonitorError(() => $"[{middleware.GetTypeName()}.OnEnd] 发生异常.{ ex.Message}.");
                    logger.Exception(ex);
                }
                finally
                {
                    FlowTracer.EndDebugStepMonitor();
                }
            }
            FlowTracer.EndStepMonitor();
        }
        #endregion
    }
}