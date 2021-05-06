using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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

            ZeroAppOption.Instance.Services.Regist("Kafka", nameof(KafkaPoster), () => DependencyHelper.GetService<KafkaConsumer>());
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
            ZeroAppOption.Instance.Services.Regist("Kafka", nameof(KafkaPoster), () => DependencyHelper.GetService<KafkaConsumer>());
        }
    }
}
