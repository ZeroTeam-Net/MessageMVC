using System;
using Agebull.Common.Ioc;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Agebull.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Rabbit
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .UseMessageMVC(true, services =>
                {
                    services.AddMessageMvcRabbitMQ();
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddHostedService<TestHost>();
                });
            await builder.Build().RunAsync();
        }
    }
}
