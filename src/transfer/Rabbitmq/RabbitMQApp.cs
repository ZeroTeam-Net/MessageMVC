using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    /// RabbitMQ
    /// </summary>
    public static class RabbitMQApp
    {
        /// <summary>
        /// 使用RabbitMQ
        /// </summary>
        /// <param name="services"></param>
        public static void AddMessageMvcRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton<IZeroOption>(pri => RabbitMQOption.Instance);
            services.AddSingleton<IHealthCheck>(RabbitMQPoster.Instance);
            services.AddSingleton<IMessagePoster>(RabbitMQPoster.Instance);//采用RabbitMQ生产端
            services.AddNameTransient<IMessageConsumer, RabbitMQConsumer>();//采用RabbitMQ消费客户端

            ZeroAppOption.Instance.Services.Regist("RabbitMQ", nameof(RabbitMQPoster), () => DependencyHelper.GetService<RabbitMQConsumer>());
        }
        /// <summary>
        /// 使用RabbitMQ
        /// </summary>
        /// <param name="services"></param>
        public static void AddMessageMvcRabbitMQClient(this IServiceCollection services)
        {
            services.AddSingleton<IZeroOption>(pri => RabbitMQOption.Instance);
            services.AddSingleton<IHealthCheck>(RabbitMQPoster.Instance);
            services.AddSingleton<IMessagePoster>(RabbitMQPoster.Instance);//采用RabbitMQ生产端

            ZeroAppOption.Instance.Services.Regist("RabbitMQ", nameof(RabbitMQPoster), () => DependencyHelper.GetService<RabbitMQConsumer>());
        }
    }
}
