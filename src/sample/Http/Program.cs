using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tcp;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                    services.AddMessageMvcHttp();
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
                Route = "*",
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