using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .UseMessageMVC(true, services =>
                {
                services.AddMessageMvcKafka();
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddHostedService<TestHost>();
                });
            await builder.Build().RunAsync();
        }
    }
}
