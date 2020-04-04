using System;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Web;
using ZeroTeam.MessageMVC.ZeroApis;

namespace WebNotifyTest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            IocHelper.ServiceCollection = services;
            services.AddTransient<IMessageMiddleware, WebSocketNotify>();
            IocHelper.SetServiceCollection(services);
            services.UseCsRedis();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            WebSocketNotify.Binding(app);
            IocHelper.ServiceCollection.UseFlow();
            app.UseStaticFiles();
            app.UseDefaultFiles("/index.htm");

            //Task.Run(Test);
        }


        static async void Test()
        {
            int left = 0;
            int join = short.MaxValue;
            while (true)
            {
                await Task.Delay(1000);
                try
                {
                    await MessagePoster.PublishAsync("MarkPoint", "real", new
                    {
                        left = left++,
                        join = join--
                    });
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                }
            }
        }
    }

}
