using CSRedis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 启动类
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            RedisHelper.Initialization(new CSRedisClient(RedisOption.Instance.ConnectionString));
            services.AddSingleton<IFlowMiddleware>(CsRedisPoster.Instance);//Redis环境准备
            services.AddSingleton<IMessagePoster>(CsRedisPoster.Instance);//Redis发布
            services.AddTransient<INetEvent, CSRedisConsumer>();//Redis订阅

            HttpRoute.Initialize(services);
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseStaticFiles();
            app.UseDefaultFiles("/index.htm");
            app.Run(HttpRoute.Call); 
        }
    }
}