using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<IHealthCheck>(RabbitMQFlow.Instance);
            services.AddSingleton<IFlowMiddleware>(RabbitMQFlow.Instance);//RabbitMQ环境
            services.AddSingleton<IMessagePoster, RabbitMQPoster>();//采用RabbitMQ生产端
            services.AddNameTransient<IMessageConsumer, RabbitMQConsumer>();//采用RabbitMQ消费客户端
        }
    }
}
