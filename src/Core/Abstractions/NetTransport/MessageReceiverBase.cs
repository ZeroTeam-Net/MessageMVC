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

        StationStateType state;

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
            MessagePoster.RegistPoster(this, Service.ServiceName);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            LogRecorder.MonitorTrace($"[{GetType().GetTypeName()}.Post] 进入本地隧道处理模式");
            //如此做法,避免上下文混乱
            var task = new TaskCompletionSource<IMessageResult>();
            Task.Factory.StartNew(() => MessageProcessor.OnMessagePush(Service, message, message.Content != null, task));
            return task.Task;
        }

        #region 序列化

        ///<inheritdoc/>
        object ISerializeProxy.Serialize(object soruce)
        {
            return Service.Serialize.Serialize(soruce);
        }

        ///<inheritdoc/>
        object ISerializeProxy.Deserialize(object soruce, Type type)
        {
            return Service.Serialize.Deserialize(soruce, type);
        }


        ///<inheritdoc/>
        T ISerializeProxy.ToObject<T>(string soruce)
        {
            return Service.Serialize.ToObject<T>(soruce);
        }


        ///<inheritdoc/>
        object ISerializeProxy.ToObject(string soruce, Type type)
        {
            return Service.Serialize.ToObject(soruce, type);
        }

        ///<inheritdoc/>
        string ISerializeProxy.ToString(object obj, bool indented)
        {
            return Service.Serialize.ToString(obj, indented);
        }
        #endregion
    }
}