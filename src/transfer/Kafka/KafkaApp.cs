using Agebull.Common.Ioc;
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
            KafkaOption.haseProducer = true;
            KafkaOption.haseConsumer = true;
            services.AddSingleton<IZeroOption>(pri => KafkaOption.Instance);
            services.AddSingleton<IHealthCheck>(KafkaPoster.Instance);
            services.AddSingleton<IMessagePoster>(KafkaPoster.Instance);//采用Kafka生产端
            services.AddNameTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
        }

        /// <summary>
        /// 使用Kafka发送器
        /// </summary>
        /// <param name="services"></param>
        public static void AddMessageMvcKafkaPoster(this IServiceCollection services)
        {
            KafkaOption.haseProducer = true;
            services.AddSingleton<IZeroOption>(pri => KafkaOption.Instance);
            services.AddSingleton<IHealthCheck>(KafkaPoster.Instance);
            services.AddSingleton<IMessagePoster>(KafkaPoster.Instance);//采用Kafka生产端
        }

        /// <summary>
        /// 使用Kafka订阅器
        /// </summary>
        /// <param name="services"></param>
        public static void AddMessageMvcKafkaConsumer(this IServiceCollection services)
        {
            KafkaOption.haseConsumer = true;
            services.AddSingleton<IZeroOption>(pri => KafkaOption.Instance);
            services.AddNameTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
        }
    }
}
