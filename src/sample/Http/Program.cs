using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            ZeroFlowControl.Shutdown();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseConfiguration(ConfigurationHelper.Root)
                        .UseUrls(ConfigurationHelper.Root.GetSection("Http:Url").Value)
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