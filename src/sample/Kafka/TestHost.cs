using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;

namespace MicroZero.Kafka.QueueStation
{
    public class TestHost : IHostedService
    {
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await MessagePoster.PublishAsync("Test2", "Test", "agebull");
                await Task.Delay(10000);
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
