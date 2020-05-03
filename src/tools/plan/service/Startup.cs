using CSRedis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks.DataAccess;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tools;

namespace ZeroTeam.MessageMVC.PlanTasks
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

            RedisHelper.Initialization(new CSRedisClient(PlanSystemOption.Instance.ConnectionString));
            ZeroFlowControl.RegistService(new ZeroService
            {
                ServiceName = "PlanManager",
                Receiver = new PlanReceiver()
            }); 
            services.AddScoped<PlanTaskDatabase>();
            services.AddSingleton<IMessageMiddleware, ReverseProxyMiddleware>();//通过反向代理组件处理计划任务消息发送

            services.UseHttp();
            services.UseCsRedis();
            services.UseFlow(typeof(PlanTaskControler));
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseStaticFiles();
            app.UseFileServer();
            app.UseDefaultFiles("/index.htm");
            app.RunMessageMVC(); 
        }
    }
}