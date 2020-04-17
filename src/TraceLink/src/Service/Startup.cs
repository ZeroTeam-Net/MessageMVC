using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
using ZeroTeam.MessageMVC.MessageTraceLink.WebApi;

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
            services.UseKafka();
            DependencyHelper.AddScoped<TraceLinkDatabase>();
            DependencyHelper.AddScoped<IFlowMiddleware, HealthCheckService>();
            services.UseHttp();
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