using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个消息队列消费者
    /// </summary>
    public interface IReceiverGet
    {
        /// <summary>
        /// 服务接收器
        /// </summary>
        IMessageReceiver Receiver(string service);

        /// <summary>
        /// 服务名称
        /// </summary>
        string ServiceName { get; }

    }
}