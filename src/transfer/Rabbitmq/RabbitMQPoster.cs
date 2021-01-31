using Agebull.Common.Logging;
using RabbitMQ.Client;
using System.Text.Json;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    ///     RabbitMQ消息发布
    /// </summary>
    internal sealed class RabbitMQPoster : BackgroundPoster<QueueItem>, IMessageWorker
    {
        /// <summary>
        /// 构造
        /// </summary>
        public RabbitMQPoster()
        {
            Name = nameof(RabbitMQPoster);
            AsyncPost = RabbitMQOption.Instance.AsyncPost;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns></returns>
        protected override Task<bool> DoPost(QueueItem item)
        {
            var channel = RabbitMQFlow.Instance.GetChannel(item.Topic);
            if (channel.option == null)
            {
                Logger.Error(() => $"[异步消息投递] 构建Channel({item.Topic})失败");
                return Task.FromResult(false);
            }
            Logger.Trace(() => $"[异步消息投递] {item.ID} 正在投递消息({RabbitMQOption.Instance.HostName}:{RabbitMQOption.Instance.Port})");
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(item.Message);
            //发送消息
            switch (channel.option.WrokType)
            {
                case RabbitMQWrokType.Topic:
                case RabbitMQWrokType.Direct:
                    channel.model.BasicPublish(exchange: channel.option.ExchangeName, routingKey: item.Topic, basicProperties: null, body: body);
                    break;
                case RabbitMQWrokType.Fanout:
                    channel.model.BasicPublish(exchange: channel.option.ExchangeName, routingKey: null, basicProperties: null, body: body);
                    break;
                default:
                    channel.model.BasicPublish(exchange: "", routingKey: item.Topic, basicProperties: null, body: body);
                    break;
            }
            return Task.FromResult(true);
        }
    }
}
