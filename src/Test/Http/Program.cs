using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using System;
using Microsoft.Extensions.Hosting;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            LogRecorder.LogPath = Path.Combine(Environment.CurrentDirectory, "logs", ConfigurationManager.Root["AppName"]);

            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        var option = ConfigurationManager.Get("Logging");
                        builder.AddConfiguration(ConfigurationManager.Root.GetSection("Logging"));
                        if (option.GetBool("console", true))
                            builder.AddConsole();
                        if (option.GetBool("innerLogger", false))
                        {
                            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());
                            LoggerProviderOptions.RegisterProviderOptions<TextLoggerOption, TextLoggerProvider>(builder.Services);
                        }
                    })
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