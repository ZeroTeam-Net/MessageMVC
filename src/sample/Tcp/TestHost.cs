using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Tcp.Sample
{
    public class TestHost : IHostedService
    {
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await MessagePoster.PublishAsync("tcp", "test", "agebull");
                await Task.Delay(10000);
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
