using Agebull.Common.Ioc;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            IocHelper.ServiceCollection.UseKafka();
            await IocHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            _ = Task.Run(Test);

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
        static async void Test()
        {
            for (int i = 0; ZeroFlowControl.CanDo && i < 10; i++)
            {
                await Task.Delay(100);

                await MessagePoster.PublishAsync("test1", "test/res", "agebull");
                await MessagePoster.PublishAsync("test1", "test/arg", "{'Value':'test'}");
                await MessagePoster.PublishAsync("test1", "test/full", "1");
                await MessagePoster.PublishAsync("test1", "test/void", "agebull");

                await MessagePoster.PublishAsync("test1", "async/res", "{'Value':'test'}");
                await MessagePoster.PublishAsync("test1", "async/arg", "{'Value':'test'}");
                await MessagePoster.PublishAsync("test1", "async/full", "{'Value':'test'}");
                await MessagePoster.PublishAsync("test1", "async/void", "{'Value':'test'}");
            }
        }
    }
}
