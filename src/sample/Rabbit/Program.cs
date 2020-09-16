using System;
using Agebull.Common.Ioc;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Agebull.Common.Configuration;
using Microsoft.Extensions.Logging;
namespace Rabbit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DependencyHelper.ServiceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(ConfigurationHelper.Root.GetSection("Logging"));
                builder.AddConsole();
                DependencyHelper.Reload();
            });
            var services = DependencyHelper.ServiceCollection;
            services.AddRabbitMQ();
            DependencyHelper.Reload();
            _ = Test();
            await services.UseMessageMvc(typeof(Program));
        }
        static async Task Test()
        {
            for (int i = 0; ZeroAppOption.Instance.IsAlive && i < 100; i++)
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
    }
}
