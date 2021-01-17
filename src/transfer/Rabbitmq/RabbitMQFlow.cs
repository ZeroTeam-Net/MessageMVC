using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    ///     RabbitMQ流程处理
    /// </summary>
    internal class RabbitMQFlow : IFlowMiddleware, IHealthCheck
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static RabbitMQFlow Instance = new RabbitMQFlow();

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
            HealthItem item = new HealthItem
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
            HealthItem item = new HealthItem
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

        #region IFlowMiddleware 

        /// <summary>
        /// 连接对象
        /// </summary>
        internal IConnection connection;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(RabbitMQPoster);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        /// <summary>
        /// 开启
        /// </summary>
        Task ILifeFlow.Open()
        {
            IConnectionFactory conFactory = new ConnectionFactory//创建连接工厂对象
            {
                HostName = RabbitMQOption.Instance.HostName,//IP地址
                Port = RabbitMQOption.Instance.Port,//端口号
                UserName = RabbitMQOption.Instance.UserName,//用户账号
                Password = RabbitMQOption.Instance.Password//用户密码
            };
            connection = conFactory.CreateConnection();//创建连接对象
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Close()
        {
            try
            {
                connection?.Close();
                connection?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            connection = null;
            return Task.CompletedTask;
        }

        #endregion
    }
}
