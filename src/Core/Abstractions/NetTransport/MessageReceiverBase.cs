﻿using Microsoft.Extensions.Logging;
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
    public class MessageReceiverBase
    {
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
        public StationStateType State { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            State = StationStateType.Initialized;
            MessagePoster.RegistPoster(this, Service.ServiceName);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task<IInlineMessage> Post(IMessageItem message)
        {
            var inline = message.ToInline();
            await MessageProcessor.OnMessagePush(Service, inline, MessageProcessor.DefaultOriginal);
            return inline;
        }


        #region 序列化

        ///<inheritdoc/>
        public object Serialize(object soruce)
        {
            return Service.Serialize.Serialize(soruce);
        }

        ///<inheritdoc/>
        public object Deserialize(object soruce, Type type)
        {
            return Service.Serialize.Deserialize(soruce, type);
        }


        ///<inheritdoc/>
        public T ToObject<T>(string soruce)
        {
            return Service.Serialize.ToObject<T>(soruce);
        }


        ///<inheritdoc/>
        public object ToObject(string soruce, Type type)
        {
            return Service.Serialize.ToObject(soruce, type);
        }

        ///<inheritdoc/>
        public string ToString(object obj,bool indented)
        {
            return Service.Serialize.ToString(obj, indented);
        }
        #endregion
    }
}