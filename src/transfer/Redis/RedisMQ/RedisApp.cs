using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// RedisMQ
    /// </summary>
    public static class RedisApp
    {
        /// <summary>
        /// 使用RedisMQ
        /// </summary>
        public static void AddMessageMvcRedis(this IServiceCollection services)
        {
            services.AddSingleton<IZeroOption>(RedisOption.Instance);
            services.AddSingleton<IHealthCheck>(CsRedisPoster.Instance);
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布

            services.AddNameTransient<INetEvent, CSRedisEventReceiver>();//Redis订阅
        }

        /// <summary>
        /// 使用Redis事件
        /// </summary>
        public static void AddMessageMvcRedisEventClient(this IServiceCollection services)
        {
            services.AddSingleton<IZeroOption>(RedisOption.Instance);
            services.AddSingleton<IHealthCheck>(CsRedisPoster.Instance);
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布
        }
    }
}
