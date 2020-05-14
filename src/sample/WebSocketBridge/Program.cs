using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ZeroTeam.MessageMVC;

namespace WebNotifyTest
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
                        .UseUrls(ConfigurationHelper.Root.GetSection("Kestrel:Endpoints:Http:Url").Value)
                        .UseKestrel((ctx, opt) =>
                        {
                            opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                        })
                        .UseStartup<Startup>();
                });
    }
}
