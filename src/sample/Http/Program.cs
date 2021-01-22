using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ZeroTeam.MessageMVC.RedisMQ;
using Microsoft.Extensions.Logging;
namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureLogging((ctx, builder) =>
                        {
                            builder.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                        })
                            .UseMessageMVC(services =>
                            {
                                services.AddMessageMvcHttp();
                                services.AddMessageMvcRedis();
                            })
                            //.UseUrls(webBuilder.Configuration.GetSection("Http:Url").Value)
                            .UseKestrel((ctx, opt) =>
                            {
                                //opt.ConfigureEndpointDefaults(lo =>
                                //{
                                //    lo.UseConnectionLogging();
                                //    //Microsoft.AspNetCore.Server.Kestrel.Core.Internal.LoggingConnectionMiddleware
                                //});
                                opt.Configure(ctx.Configuration.GetSection("Http:Kestrel"));
                            })
                            .UseStartup<Startup>();
                });
    }
}