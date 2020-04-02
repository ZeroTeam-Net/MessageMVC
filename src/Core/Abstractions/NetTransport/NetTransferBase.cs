using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 网络传输对象基类
    /// </summary>
    /// <remarks>
    /// 实现了IMessagePoster自注册,可以做到本进程调用不会提升到网络层面
    /// </remarks>
    public class NetTransferBase : IMessagePoster
    {
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
        void IMessagePoster.Initialize()
        {
            State = StationStateType.Initialized;
            MessagePoster.RegistPoster(this, Service.ServiceName);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<(MessageState state, string result)> IMessagePoster.Post(IMessageItem message)
        {
            await MessageProcessor.OnMessagePush(Service, message, null);
            return (message.State, message.Result);
        }
    }
}