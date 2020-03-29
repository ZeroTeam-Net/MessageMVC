using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// 通过ZMQ实现的进程内通讯
    /// </summary>
    public static class RedisHelper
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        public static void UseCsRedis(this IServiceCollection services)
        {
            services.AddSingleton<IFlowMiddleware>(CsRedisProducer.Instance);//Redis环境准备
            services.AddSingleton<IMessageProducer>(CsRedisProducer.Instance);//Redis发布
            services.AddTransient<INetEvent, CSRedisConsumer>();//Redis订阅
        }
    }
}
