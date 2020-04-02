using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    /// KafkaMvc
    /// </summary>
    public static class KafkaMessageMVC
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        /// <param name="services"></param>
        public static void UseKafka(this IServiceCollection services)
        {
            services.AddTransient<IFlowMiddleware, KafkaProducer>();//Kafka环境
            services.AddTransient<IMessagePoster, KafkaProducer>();//采用Kafka生产端
            services.AddTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
        }
    }
}
