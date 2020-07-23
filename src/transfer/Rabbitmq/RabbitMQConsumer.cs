﻿using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    /// RabbitMQ消息队列消费者
    /// </summary>
    internal class RabbitMQConsumer : MessageReceiverBase, IMessageConsumer
    {
        /// <summary>
        /// 构造
        /// </summary>
        public RabbitMQConsumer() : base(nameof(RabbitMQConsumer))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(RabbitMQPoster);

        /// <summary>
        /// 当前通道
        /// </summary>
        private IModel channel;
        /// <summary>
        /// 消费对象
        /// </summary>
        EventingBasicConsumer consumer;
        /// <summary>
        /// 配置
        /// </summary>
        RabbitMQItemOption Option;
        /// <summary>
        /// 轮询的Task
        /// </summary>
        TaskCompletionSource<bool> completionSource;

        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                OnMessagePush(ea);
            };
            //消费者开启监听
            channel.BasicConsume(queue: Service.ServiceName, autoAck: false, consumer: consumer);
            completionSource = new TaskCompletionSource<bool>();
            return completionSource.Task;
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="arg"></param>
        private void OnMessagePush(BasicDeliverEventArgs arg)
        {
            var json = Encoding.UTF8.GetString(arg.Body.ToArray());

            if (SmartSerializer.TryToMessage(json, out var message))
            {
                var task = MessageProcessor.OnMessagePush(Service, message, true, arg);//BUG:应该配置化同步或异步
                task.Wait();
            }
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageReceiver.OnResult(IInlineMessage item, object tag)
        {
            var ea = (BasicDeliverEventArgs)tag;
            try
            {
                channel.BasicAck(ea.DeliveryTag, true);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.Close()
        {
            completionSource.SetResult(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接对象
        /// </summary>
        IConnection connection;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            if (!RabbitMQOption.Instance.ItemOptions.TryGetValue(Service.ServiceName, out Option))
            {
                Option = new RabbitMQItemOption();
            }
            try
            {

                IConnectionFactory conFactory = new ConnectionFactory//创建连接工厂对象
                {
                    HostName = RabbitMQOption.Instance.HostName,//IP地址
                    Port = RabbitMQOption.Instance.Port,//端口号
                    UserName = RabbitMQOption.Instance.UserName,//用户账号
                    Password = RabbitMQOption.Instance.Password//用户密码
                };
                connection = conFactory.CreateConnection();//创建连接对象

                channel = connection.CreateModel();//创建连接会话对象

                switch (Option.WrokType)
                {
                    case RabbitMQWrokType.Fanout:
                        {
                            //声明交换机
                            channel.ExchangeDeclare(exchange: Option.ExchangeName, type: "fanout");
                            var name = $"{Option.ExchangeName}_{RandomCode.Generate(6)}";
                            //声明队列
                            channel.QueueDeclare(
                                          queue: name,//消息队列名称
                                          durable: Option.Durable,//是否缓存
                                          exclusive: Option.Exclusive,
                                          autoDelete: Option.AutoDelete,
                                          arguments: null);

                            //将队列与交换机进行绑定
                            channel.QueueBind(queue: name, exchange: Option.ExchangeName, routingKey: "");
                        }
                        break;
                    case RabbitMQWrokType.Direct:
                        {
                            //声明交换机
                            channel.ExchangeDeclare(exchange: Option.ExchangeName, type: "direct");
                            var name = $"{Option.ExchangeName}_{RandomCode.Generate(6)}";
                            //声明队列
                            channel.QueueDeclare(
                                          queue: name,//消息队列名称
                                          durable: Option.Durable,//是否缓存
                                          exclusive: Option.Exclusive,
                                          autoDelete: Option.AutoDelete,
                                          arguments: null);
                            //匹配多个路由
                            channel.QueueBind(queue: name, exchange: Option.ExchangeName, routingKey: Service.ServiceName);
                            break;
                        }
                    case RabbitMQWrokType.Topic:
                        {
                            //声明交换机
                            channel.ExchangeDeclare(exchange: Option.ExchangeName, type: "topic");
                            var name = $"{Option.ExchangeName}_{RandomCode.Generate(6)}";
                            //声明队列
                            channel.QueueDeclare(
                                          queue: name,//消息队列名称
                                          durable: Option.Durable,//是否缓存
                                          exclusive: Option.Exclusive,
                                          autoDelete: Option.AutoDelete,
                                          arguments: null);
                            //匹配多个路由
                            channel.QueueBind(queue: name, exchange: Option.ExchangeName, routingKey: Service.ServiceName);
                        }
                        break;
                    default:
                        //声明队列
                        channel.QueueDeclare(
                                      queue: Service.ServiceName,//消息队列名称
                                      durable: Option.Durable,//是否缓存
                                      exclusive: Option.Exclusive,
                                      autoDelete: Option.AutoDelete,
                                      arguments: null);
                        break;
                }
                if (Option.Qos > 0)
                    channel.BasicQos(0, Option.Qos, false);//告诉Rabbit每次只能向消费者发送一条信息,再消费者未确认之前,不再向他发送信息
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.LoopComplete()
        {
            channel?.Close();
            channel?.Dispose();
            channel = null;
            connection?.Close();
            connection?.Dispose();
            connection = null;
            return Task.CompletedTask;
        }
    }
}