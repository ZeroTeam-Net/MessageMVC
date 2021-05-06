using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    ///     RabbitMQ消息发布
    /// </summary>
    internal sealed class RabbitMQPoster : BackgroundPoster<QueueItem>, IMessageWorker, ILifeFlow, IZeroDiscover, IHealthCheck
    {
        #region Poster

        /// <summary>
        /// 单例
        /// </summary>
        public static RabbitMQPoster Instance = new();

        /// <summary>
        /// 征集周期管理器
        /// </summary>
        protected override ILifeFlow LifeFlow => this;

        /// <summary>
        /// 构造
        /// </summary>
        public RabbitMQPoster()
        {
            Name = nameof(RabbitMQPoster);
        }

        protected override TaskCompletionSource<IMessageResult> DoPost(QueueItem item)
        {
            Logger.Trace(() => $"[异步消息投递] {item.ID} 正在投递消息({RabbitMQOption.Instance.HostName}:{RabbitMQOption.Instance.Port})");
            var res = new TaskCompletionSource<IMessageResult>();
            var channel = GetChannel(item.Topic);
            if (channel.option == null)
            {
                Logger.Error(() => $"[异步消息投递] 构建Channel({item.Topic})失败");

                res.TrySetResult(new MessageResult
                {
                    ID = item.Message.ID,
                    Trace = item.Message.TraceInfo,
                    State = MessageState.Cancel
                });
                return res;
            }

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
            return res;
        }

        /// <summary>
        /// 连接对象
        /// </summary>
        internal IConnection connection;

        /// <summary>
        /// 发现期间开启任务
        /// </summary>
        Task IZeroDiscover.Discovery()
        {
            IConnectionFactory conFactory = new ConnectionFactory//创建连接工厂对象
            {
                HostName = RabbitMQOption.Instance.HostName,//IP地址
                Port = RabbitMQOption.Instance.Port,//端口号
                UserName = RabbitMQOption.Instance.UserName,//用户账号
                Password = RabbitMQOption.Instance.Password//用户密码
            };
            connection = conFactory.CreateConnection();//创建连接对象
            AsyncPost = RabbitMQOption.Instance.AsyncPost;
            DoStart();
            Logger.Information($"{Name}已开启");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
       async Task ILifeFlow.Destroy()
        {
            await Destroy();
            try
            {
                foreach (var (model, _) in channels.Values)
                {
                    model.Close();
                    model.Dispose();
                }
                channels.Clear();

                connection?.Close();
                connection?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            connection = null;
            RecordLog(LogLevel.Information, $"{Name}已关闭");
        }

        #endregion
        #region IHealthCheck

        Task<HealthInfo> IHealthCheck.Check()
        {
            var info = new HealthInfo
            {
                ItemName = nameof(RabbitMQPoster),
                Items = new List<HealthItem>()
            };

            info.Items.Add(ProduceTest());
            info.Items.Add(ConsumerTest());
            return Task.FromResult(info);
        }

        private static HealthItem ProduceTest()
        {
            HealthItem item = new()
            {
                ItemName = "Produce"
            };
            try
            {
                IConnectionFactory conFactory = new ConnectionFactory//创建连接工厂对象
                {
                    HostName = RabbitMQOption.Instance.HostName,//IP地址
                    Port = RabbitMQOption.Instance.Port,//端口号
                    UserName = RabbitMQOption.Instance.UserName,//用户账号
                    Password = RabbitMQOption.Instance.Password//用户密码
                };
                using var con = conFactory.CreateConnection();//创建连接对象
                using var channel = con.CreateModel();//创建连接会话对象
                channel.QueueDeclare(
                          queue: "HealthCheck",//消息队列名称
                          durable: false,//是否缓存
                          exclusive: false,
                          autoDelete: false,
                          arguments: null);

                byte[] body = Encoding.UTF8.GetBytes("HealthCheck Test");
                //发送消息
                DateTime start = DateTime.Now;
                channel.BasicPublish(exchange: "", routingKey: "HealthCheck", basicProperties: null, body: body);


                item.Value = (DateTime.Now - start).TotalMilliseconds;
                item.Details = "Success";
                item.Level = 5;
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }

        private HealthItem ConsumerTest()
        {
            HealthItem item = new()
            {
                ItemName = "Consumer"
            };
            try
            {
                //var config = RabbitMQOption.Instance.CopyConsumer();
                //config.GroupId = ZeroAppOption.Instance.AppName;
                //var builder = new ConsumerBuilder<Ignore, string>(config);
                //using var consumer = builder.Build();
                //consumer.Subscribe(RabbitMQOption.Instance.TestTopic);

                //DateTime start = DateTime.Now;
                //var re = consumer.Consume(new TimeSpan(0, 0, 3));

                //item.Value = (DateTime.Now - start).TotalMilliseconds;
                //if (item.Value < 10)
                //    item.Level = 5;
                //else if (item.Value < 100)
                //    item.Level = 4;
                //else if (item.Value < 500)
                //    item.Level = 3;
                //else if (item.Value < 3000)
                //    item.Level = 2;
                //else item.Level = 1;
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }
        #endregion

        #region Channel

        /// <summary>
        /// 当前通道
        /// </summary>
        internal readonly ConcurrentDictionary<string, (IModel model, RabbitMQItemOption option)> channels = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 取得通道
        /// </summary>
        /// <param name="name">消息</param>
        /// <returns></returns>
        internal (IModel model, RabbitMQItemOption option) GetChannel(string name)
        {
            if (channels.TryGetValue(name, out var item))
            {
                return item;
            }
            if (!RabbitMQOption.Instance.ItemOptions.TryGetValue(name, out var opt))
            {
                opt = new RabbitMQItemOption
                {
                    WrokType = RabbitMQWrokType.Default
                };
            }
            try
            {
                var channel = connection.CreateModel();//创建连接会话对象
                switch (opt.WrokType)
                {
                    case RabbitMQWrokType.Fanout:
                        channel.ExchangeDeclare(exchange: opt.ExchangeName, type: "fanout");
                        break;
                    case RabbitMQWrokType.Direct:
                        channel.QueueDeclare(
                                  queue: name,//消息队列名称
                                  durable: opt.Durable,//是否缓存
                                  exclusive: opt.Exclusive,
                                  autoDelete: opt.AutoDelete,
                                  arguments: null);
                        channel.ExchangeDeclare(exchange: opt.ExchangeName, type: "direct");
                        break;
                    case RabbitMQWrokType.Topic:
                        channel.QueueDeclare(
                                  queue: name,//消息队列名称
                                  durable: opt.Durable,//是否缓存
                                  exclusive: opt.Exclusive,
                                  autoDelete: opt.AutoDelete,
                                  arguments: null);
                        channel.ExchangeDeclare(exchange: opt.ExchangeName, type: "topic");
                        break;
                }
                if (!channels.TryAdd(name, (channel, opt)))
                    channel.Dispose();
                return channels[name];
            }
            catch (Exception ex)
            {
                ScopeRuner.ScopeLogger.Error(() => $"[异步消息投递] 构建Channel失败,{ex}");
            }

            return (null, null);
        }

        #endregion
    }
}
