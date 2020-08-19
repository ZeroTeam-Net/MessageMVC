using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// RedisMQ
    /// </summary>
    public static class RedisApp
    {
        static bool isUsed;
        /// <summary>
        /// 使用RedisMQ
        /// </summary>
        public static void AddCsRedis(this IServiceCollection services)
        {
            if (isUsed)
                return;
            isUsed = true;

            services.AddSingleton<IFlowMiddleware>(RedisFlow.Instance);
            services.AddSingleton<IHealthCheck>(RedisFlow.Instance);

            services.TryAddTransient<INetEvent, CSRedisEventReceiver>();//Redis订阅
            services.TryAddTransient<IMessageConsumer, CSRedisQueueReceiver>();//Redis订阅
            services.AddSingleton<IMessagePoster, CsRedisQueuePoster>();//Redis发布
            services.AddSingleton<IMessagePoster, CsRedisEventPoster>();//Redis发布
        }

        /// <summary>
        /// 使用RedisMQ
        /// </summary>
        public static void AddRedisPoster(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(RedisFlow.Instance);
            services.AddSingleton<IHealthCheck>(RedisFlow.Instance);

            services.AddSingleton<IMessagePoster, CsRedisQueuePoster>();//Redis发布
            services.AddSingleton<IMessagePoster, CsRedisEventPoster>();//Redis发布
        }

        /// <summary>
        /// 使用Redis消息队列
        /// </summary>
        public static void AddRedisQueue(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(RedisFlow.Instance);
            services.AddSingleton<IHealthCheck>(RedisFlow.Instance);
            services.TryAddTransient<IMessageConsumer, CSRedisQueueReceiver>();//Redis订阅
            services.AddSingleton<IMessagePoster, CsRedisQueuePoster>();//Redis发布
        }

        /// <summary>
        /// 使用Redis消息队列
        /// </summary>
        public static void AddRedisQueueClient(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(RedisFlow.Instance);
            services.AddSingleton<IHealthCheck>(RedisFlow.Instance);
            services.AddSingleton<IMessagePoster, CsRedisQueuePoster>();//Redis发布
        }

        /// <summary>
        /// 使用Redis事件
        /// </summary>
        public static void AddRedisEvent(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(RedisFlow.Instance);
            services.AddSingleton<IHealthCheck>(RedisFlow.Instance);
            services.TryAddTransient<INetEvent, CSRedisEventReceiver>();//Redis订阅
            services.AddSingleton<IMessagePoster, CsRedisEventPoster>();//Redis发布
        }

        /// <summary>
        /// 使用Redis事件
        /// </summary>
        public static void AddRedisEventClient(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(RedisFlow.Instance);
            services.AddSingleton<IHealthCheck>(RedisFlow.Instance);
            services.AddSingleton<IMessagePoster, CsRedisEventPoster>();//Redis发布
        }
    }
}
