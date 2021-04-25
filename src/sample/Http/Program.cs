using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tcp;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseMessageMVC(services =>
                {
                    services.AddMessageMvcTcpClient();
                    services.AddMessageMvcHttpClient();
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

        static void RetistTest()
        {
            var service = new ZeroService
            {
                IsAutoService = true,
                ServiceName = "MyTest",
                Receiver = new EmptyReceiver(),
                Serialize = new NewtonJsonSerializeProxy()
            } as IService;

            service.RegistWildcardAction(new ApiActionInfo
            {
                Name = "*",
                Routes =new []{ "*" },
                ControllerName = "DataEventProxy",
                ControllerCaption = "DataEventProxy",
                AccessOption = ApiOption.Public | ApiOption.Anymouse,
                ResultType = typeof(Task),
                IsAsync = true,
                Action = (msg, seri, arg) => null
            });
            ZeroFlowControl.RegistService(service);
        }
    }

    public interface ITest
    {
        string Name { get; set; }
    }
    public class TestObject : ITest
    {
        public string Name { get; set; }
    }
}