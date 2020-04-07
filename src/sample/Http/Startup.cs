using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;

namespace ZeroTeam.MessageMVC.Http
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
            //services.UseCsRedis();
            //services.AddSingleton<IFlowMiddleware, KafkaPoster>();//Kafka环境
            //services.AddSingleton<IMessagePoster, KafkaPoster>();//采用Kafka生产端
            services.AddSingleton<IFlowMiddleware, CsRedisPoster>();//采用Redis生产端
            services.AddSingleton<IMessagePoster, CsRedisPoster>();//采用Redis生产端
            HttpRoute.Initialize(services);
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.Run(HttpRoute.Call); 
        }
    }
}