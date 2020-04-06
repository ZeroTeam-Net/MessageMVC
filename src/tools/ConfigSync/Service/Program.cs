using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ZeroTeam.MessageMVC.ConfigSync
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
                        .UseConfiguration(ConfigurationManager.Root)
                        .UseUrls(ConfigurationManager.Root.GetSection("Kestrel.Endpoints.Http.Url").Value)
                        .UseKestrel((ctx, opt) =>
                        {
                            opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                        })
                        .UseStartup<Startup>();
                });
    }
}