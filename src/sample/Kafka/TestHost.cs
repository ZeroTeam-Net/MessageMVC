using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace MicroZero.Kafka.QueueStation
{
    public class TestHost : IHostedService
    {
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    await Task.Delay(100);

            //    await MessagePoster.PublishAsync("test1", "test/res", "agebull");
            //    await MessagePoster.PublishAsync("test1", "test/arg", "{'Value':'test'}");
            //    await MessagePoster.PublishAsync("test1", "test/full", "{'name':'test'}");
            //    await MessagePoster.PublishAsync("test1", "test/void", "agebull");

            //    await MessagePoster.PublishAsync("test1", "async/res", "{'Value':'test'}");
            //    await MessagePoster.PublishAsync("test1", "async/arg", "{'Value':'test'}");
            //    await MessagePoster.PublishAsync("test1", "async/full", "{'Value':'test'}");
            //    await MessagePoster.PublishAsync("test1", "async/void", "{'Value':'test'}");
            //}
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
