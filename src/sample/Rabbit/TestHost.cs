using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Rabbit
{
    public class TestHost : IHostedService
    {
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!ZeroAppOption.Instance.IsRuning)
                {
                    await Task.Delay(1000);
                    continue;
                }
                await Task.Delay(100);
                //await Task.Delay(1);
                //await MessagePoster.PublishAsync("test1", "test/res", "agebull");
                //await MessagePoster.PublishAsync("test1", "test/arg", "{'Value':'test'}");
                await MessagePoster.PublishAsync("test1", "test/full", "{'name':'test'}");
                //await MessagePoster.PublishAsync("test1", "test/void", "agebull");

                //await MessagePoster.PublishAsync("test1", "async/res", "{'Value':'test'}");
                //await MessagePoster.PublishAsync("test1", "async/arg", "{'Value':'test'}");
                //await MessagePoster.PublishAsync("test1", "async/full", "{'Value':'test'}");
                //await MessagePoster.PublishAsync("test1", "async/void", "{'Value':'test'}");
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
