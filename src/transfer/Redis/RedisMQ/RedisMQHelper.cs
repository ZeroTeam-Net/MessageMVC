using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// RedisMQ
    /// </summary>
    public static class RedisMQHelper
    {
        /// <summary>
        /// 使用RedisMQ
        /// </summary>
        public static void UseCsRedis(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(CsRedisPoster.Instance);//Redis环境准备
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布
            services.AddTransient<INetEvent, CSRedisConsumer>();//Redis订阅
            services.AddTransient<IMessageConsumer, CSRedisConsumer>();//Redis订阅
        }
    }
}
