using System;
using Agebull.Common.Ioc;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Agebull.Common.Configuration;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.RedisMQ;

namespace Rabbit
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .UseMessageMVC(true, services =>
                {
                    services.AddMessageMvcKafka();
                    services.AddMessageMvcHttp();
                    services.AddMessageMvcRedis();
                    services.AddMessageMvcRabbitMQ();
                });
            await builder.Build().RunAsync();
        }
    }
}
