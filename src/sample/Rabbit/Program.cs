using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.RabbitMQ;

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
