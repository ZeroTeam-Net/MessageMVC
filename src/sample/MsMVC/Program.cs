using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MsMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseKestrel((ctx, opt) =>
                    {
                        opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                    })
                    .UseStartup<Startup>();
                });
    }
}
