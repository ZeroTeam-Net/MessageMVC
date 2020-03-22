using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Web;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace WebNotifyTest
{
    public class Startup
    {
        IServiceCollection Services;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            Services = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Services.UseZeroMQInporc();
            Services.AddTransient<IMessageMiddleware, WebSocketNotify>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            WebSocketNotify.Binding(app);
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            Services.UseFlow();

            Task.Run(Test);
        }


        static void Test()
        {
            var producer = IocHelper.Create<IMessageProducer>();
            int left = 0;
            int join = short.MaxValue;
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    producer.ProducerAsync("real", "real", new
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
