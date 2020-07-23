﻿using Agebull.Common.Logging;
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
        /// 是否本地接收者
        /// </summary>
        bool IMessagePoster.IsLocalReceiver => true;

        /// <summary>
        /// 是否可用
        /// </summary>
        bool IMessagePoster.CanDo => state == StationStateType.Run;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger { protected get; set; }

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
        StationStateType IMessagePoster.State { get => state; set => state = value; }

        /// <summary>
        /// 初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            if (state >= StationStateType.Initialized)
                return;
            state = StationStateType.Initialized;
            Logger.Information(() => $"服务[{Service.ServiceName}] 使用接收器{Name}");
            MessagePoster.RegistPoster(this, Service.ServiceName);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            FlowTracer.MonitorDetails(() => $"[{GetType().GetTypeName()}.Post] 进入本地隧道处理模式");
            //如此做法,避免上下文混乱
            var task = new TaskCompletionSource<IMessageResult>();
            _ = MessageProcessor.OnMessagePush(Service, message, message.Content != null, task);
            await task.Task;

            return null;//直接使用原始消息
        }
    }
}