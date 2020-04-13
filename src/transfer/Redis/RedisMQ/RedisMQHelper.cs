using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// RedisMQ
    /// </summary>
    public static class RedisMQHelper
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
            services.AddSingleton<IFlowMiddleware>(CsRedisPoster.Instance);//Redis环境准备
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布
            services.AddTransient<INetEvent, CSRedisConsumer>();//Redis订阅
            services.AddTransient<IMessageConsumer, CSRedisQueue>();//Redis订阅
        }

        /// <summary>
        /// 使用RedisMQ
        /// </summary>
        public static void UseRedisPoster(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(CsRedisPoster.Instance);//Redis环境准备
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布
        }
    }
}
