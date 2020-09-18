using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    /// KafkaMvc
    /// </summary>
    public static class KafkaApp
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        /// <param name="services"></param>
        public static void AddMessageMvcKafka(this IServiceCollection services)
        {
            services.AddSingleton<IHealthCheck>(KafkaFlow.Instance);
            services.AddSingleton<IFlowMiddleware>(KafkaFlow.Instance);//Kafka环境
            services.AddSingleton<IMessagePoster, KafkaPoster>();//采用Kafka生产端
            services.AddTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
        }
    }
}
