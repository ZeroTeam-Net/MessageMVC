using Agebull.Common.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;

namespace ZeroTeam.MessageMVC.RabbitMQ
{

    /// <summary>
    /// 队列内容
    /// </summary>
    internal class RabbitMQQueueItem : QueueItem
    {
        /// <summary>
        /// 配置
        /// </summary>
        public RabbitMQItemOption Option { get; set; }
    }

    /// <summary>
    ///     RabbitMQ消息发布
    /// </summary>
    internal sealed class RabbitMQPoster : BackgroundPoster<RabbitMQQueueItem>
    {
        #region IMessagePoster 

        internal RabbitMQPoster()
        {
            Name = nameof(RabbitMQPoster);
        }

        /// <summary>
        /// 当前通道
        /// </summary>
        readonly ConcurrentDictionary<string, (IModel model, RabbitMQItemOption option)> channels = new ConcurrentDictionary<string, (IModel model, RabbitMQItemOption option)>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 关闭处理
        /// </summary>
        protected override Task OnClose()
        {
            cancellation.Dispose();
            cancellation = null;
            State = StationStateType.Closed;
            foreach (var (model, _) in channels.Values)
            {
                model.Close();
                model.Dispose();
            }
            channels.Clear();
            return Task.CompletedTask;
        }

        #endregion

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns></returns>
        protected override Task<bool> DoPost(RabbitMQQueueItem item)
        {
            if (!channels.TryGetValue(item.Topic, out var channel))
            {
                var (st, ch) = CreateChannel(item);
                if (!st)
                {
                    return Task.FromResult(false);
                }
                channel = ch;
            }
            Logger.Trace(() => $"[异步消息投递] 正在投递消息({RabbitMQOption.Instance.HostName}:{RabbitMQOption.Instance.Port})");
            byte[] body = Encoding.UTF8.GetBytes(item.Message);
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
            Logger.Trace(() => $"[异步消息投递] 投递成功");
            return Task.FromResult(true);
        }

        private (bool state, (IModel model, RabbitMQItemOption option)) CreateChannel(RabbitMQQueueItem item)
        {
            if (!RabbitMQOption.Instance.ItemOptions.TryGetValue(item.Topic, out var opt))
            {
                opt = new RabbitMQItemOption
                {
                    WrokType = RabbitMQWrokType.Default
                };
            }
            try
            {
                var channel = RabbitMQFlow.Instance.connection.CreateModel();//创建连接会话对象
                switch (opt.WrokType)
                {
                    case RabbitMQWrokType.Fanout:
                        channel.ExchangeDeclare(exchange: opt.ExchangeName, type: "fanout");
                        break;
                    case RabbitMQWrokType.Direct:
                        channel.QueueDeclare(
                                  queue: item.Topic,//消息队列名称
                                  durable: opt.Durable,//是否缓存
                                  exclusive: opt.Exclusive,
                                  autoDelete: opt.AutoDelete,
                                  arguments: null);
                        channel.ExchangeDeclare(exchange: opt.ExchangeName, type: "direct");
                        break;
                    case RabbitMQWrokType.Topic:
                        channel.QueueDeclare(
                                  queue: item.Topic,//消息队列名称
                                  durable: opt.Durable,//是否缓存
                                  exclusive: opt.Exclusive,
                                  autoDelete: opt.AutoDelete,
                                  arguments: null);
                        channel.ExchangeDeclare(exchange: opt.ExchangeName, type: "topic");
                        break;
                }
                channels.TryAdd(item.Topic, (channel, opt));
                return (true, (channel, opt));
            }
            catch (Exception ex)
            {
                Logger.Warning(() => $"[异步消息投递] 构建Channel失败,{ex}");
            }

            return (false, (null, null));
        }

        #endregion

    }
}
