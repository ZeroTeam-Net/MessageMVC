using Microsoft.Extensions.Hosting;
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
                //.ConfigureServices((ctx, services) =>
                //{
                //    services.AddHostedService<TestHost>();
                //})
                ;
            await builder.Build().RunAsync();
        }
    }
}
