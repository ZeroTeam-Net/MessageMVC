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
        public static void UseCsRedis(this IServiceCollection services)
        {
            if (isUsed)
                return;
            isUsed = true;
            UseRedisPoster(services);
            UseRedisQueue(services);
            UseRedisEvent(services);
        }

        /// <summary>
        /// 使用RedisMQ
        /// </summary>
        public static void UseRedisPoster(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(CsRedisPoster.Instance);//Redis环境准备
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布
            services.AddSingleton<IHealthCheck>(CsRedisPoster.Instance);
        }

        /// <summary>
        /// 使用Redis消息队列
        /// </summary>
        public static void UseRedisQueue(this IServiceCollection services)
        {
            services.TryAddTransient<IMessageConsumer, CSRedisQueue>();//Redis订阅
        }

        /// <summary>
        /// 使用Redis事件
        /// </summary>
        public static void UseRedisEvent(this IServiceCollection services)
        {
            services.TryAddTransient<INetEvent, CSRedisConsumer>();//Redis订阅
        }
    }
}
