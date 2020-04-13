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
        /// 缺省原始内容,用于区别内部直连
        /// </summary>
        public static readonly object DefaultOriginal = new object();

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
                Original = original
            };
            return process.Process();
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
            using (DependencyScope.CreateScope($"MessageProcessor : {Message.Topic}/{Message.Title}"))
            {
                Message.Reset();
                await Message.PrepareInline();
                if (await Prepare() && Message.State == MessageState.None)
                    await DoHandle();

                Message.ResultSerializer ??= Service.Serialize;

                await PushResult();
                await OnEnd();
            }
        }

        /// <summary>
        /// 准备处理
        /// </summary>
        /// <returns>是否需要正式处理</returns>
        private async Task<bool> Prepare()
        {
            index = 0;
            try
            {
                middlewares = DependencyHelper.ServiceProvider.GetServices<IMessageMiddleware>().OrderBy(p => p.Level).ToArray();

                foreach (var middleware in middlewares)
                {
                    middleware.Processor = this;
                }

                foreach (var middleware in middlewares.Where(p => p.Scope.HasFlag(MessageHandleScope.Prepare)))
                {
                    if (!await middleware.Prepare(Service, Message, Original))
                        return false;
                }
                if (GlobalContext.CurrentNoLazy == null)
                    GlobalContext.Current.Message = Message;
                return true;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                Message.State = MessageState.Error;
                return false;
            }
        }

        /// <summary>
        /// 中间件链式处理
        /// </summary>
        /// <returns></returns>
        private async Task DoHandle()
        {
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
                LogRecorder.Trace("Message handle({0})", next.GetTypeName());
                await next.Handle(Service, Message, Original, Handle);
                return;
            }
        }

        #endregion

        #region 处理结果返回

        /// <summary>
        /// 结果推到调用处
        /// </summary>
        private async Task PushResult()
        {
            Message.PrepareOffline();
            if (Original == DefaultOriginal)//内部自调用,无需处理
            {
                return;
            }
            try
            {
                LogRecorder.Trace("Message OnResult({0})", Service.Receiver.GetTypeName());
                await Service.Receiver.OnResult(Message, Original);
            }
            catch (Exception ex)
            {
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
            foreach (var middleware in array)
            {
                try
                {
                    LogRecorder.Trace("Message OnMessageError({0})", middleware.GetTypeName());
                    await middleware.OnGlobalException(Service, Message, Original);
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                }
            }
            if (Message.ResultData != null)
                return;
            if (Message.ResultCreater != null)
            {
                Message.ResultData = Message.ResultCreater(Message.RuntimeStatus.Code, Message.RuntimeStatus.Message);
            }
            else if (Message.RuntimeStatus != null)
            {
                Message.ResultData = Message.RuntimeStatus;
            }
            else
            {
                Message.ResultOutdated = false;
                Message.Result = ex.Message;
            }
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
            Message.Offline(null);
            foreach (var middleware in array)
            {
                try
                {
                    LogRecorder.Trace("Message OnResult({0})", middleware.GetTypeName());
                    await middleware.OnEnd(Message);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
            }
        }
        #endregion
    }
}