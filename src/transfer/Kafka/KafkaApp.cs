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
        public static void UseKafka(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(KafkaPoster.Instance);//Kafka环境
            services.AddSingleton<IMessagePoster>(KafkaPoster.Instance);//采用Kafka生产端
            services.AddTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
            services.AddSingleton<IHealthCheck>(KafkaPoster.Instance);
        }
    }
}
