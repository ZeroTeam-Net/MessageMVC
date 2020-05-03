using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Kafka;
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
            services.UseCsRedis();
            services.UseKafka();
            //if (GatewayOption.Instance.EnableSecurityChecker)
            //    services.AddTransient<IMessageMiddleware, SecurityChecker>();
            //if (GatewayOption.Instance.EnableCache)
            //    services.AddTransient<IMessageMiddleware, RouteCache>();
            //if (GatewayOption.Instance.EnableWxPay)
            //    services.AddTransient<IMessageMiddleware, WxPayRouter>();

            services.UseHttp();
            services.UseFlow(typeof(Startup));
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseFileServer();

            app.RunMessageMVC(); 
        }
    }
}