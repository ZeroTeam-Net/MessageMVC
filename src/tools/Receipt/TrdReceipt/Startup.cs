using Agebull.Common.Configuration;
using CSRedis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            var con = ConfigurationManager.Root.GetSection("MessageMVC:Redis:ConnectionString").Value;
            RedisHelper.Initialization(new CSRedisClient(con));

            services.UseHttp();
            services.UseFlow(typeof(Startup));
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="_"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.RunMessageMVC(); 
        }
    }
}