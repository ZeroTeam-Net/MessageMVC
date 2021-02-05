using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.RabbitMQ;
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
                    services.AddMessageMvcHttpClient();
                    services.AddMessageMvcRedis();
                    services.AddMessageMvcRabbitMQ();
                });
            await builder.Build().RunAsync();
        }
    }
}
