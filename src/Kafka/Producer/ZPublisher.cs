

using Agebull.MicroZero;
using ZeroTeam.MessageMVC.Kafka;

namespace ZeroTeam.MessageMVC.PubSub
{
    /// <summary>
    ///     消息发布
    /// </summary>
    internal class ZeroPublisher : IZeroPublisher
    {
        bool IZeroPublisher.Publish(string station, string title, string sub, string arg)
        {
            return MessageProducer.Publish(station, title, arg);
        }
    }
}