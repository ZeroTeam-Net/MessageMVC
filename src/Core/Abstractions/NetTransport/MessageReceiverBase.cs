using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// 消息接收对象基类
    /// </summary>
    /// <remarks>
    /// 实现了IMessagePoster自注册,可以做到本进程调用不会提升到网络层面
    /// </remarks>
    public class MessageReceiverBase : IMessagePoster
    {
        /// <summary>
        /// 内部构造
        /// </summary>
        /// <param name="name"></param>
        protected MessageReceiverBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 是否可启动本地隧道
        /// </summary>
        protected bool CanLocalTunnel { get; set; }

        /// <summary>
        /// 是否本地接收者
        /// </summary>
        bool IMessageWorker.IsLocalReceiver => true;

        /// <summary>
        /// 是否可用
        /// </summary>
        bool IMessageWorker.CanDo => state == StationStateType.Run;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// 服务
        /// </summary>
        public IService Service { get; set; }

        /// <summary>
        /// 运行状态
        /// </summary>
        protected StationStateType state;

        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType IMessageWorker.State { get => state; set => state = value; }

        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            if (state >= StationStateType.Initialized)
                return Task.CompletedTask;

            state = StationStateType.Initialized;
            Logger.Information(() => $"服务[{Service.ServiceName}] 使用接收器{Name}");
            if (CanLocalTunnel && MessagePoster.LocalTunnel)
                MessagePoster.RegistPoster(this, Service.ServiceName);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            if (!ZeroAppOption.Instance.IsAlive)
            {
                message.State = MessageState.Cancel;
                return null;
            }
            if (CanLocalTunnel && MessagePoster.LocalTunnel)
            {
                FlowTracer.MonitorDetails(() => $"[{GetType().GetTypeName()}.Post] 进入本地隧道处理模式");
                await MessageProcessor.OnMessagePush(Service, message, message.Argument != null, null);
            }
            return null;//直接使用原始消息
        }
    }
}