using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            IocHelper.ServiceCollection.UseZeroMQInporc();
            await IocHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            await Test();

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
        static async Task Test()
        {
            //var MessagePoster = IocHelper.Create<IMessagePoster>();
            for (int i = 0; ZeroFlowControl.IsRuning && i < 10; i++)
            {
                await Task.Delay(100);
                MessagePoster.Publish("Inproc", "test/res", "agebull");
                MessagePoster.Publish("Inproc", "test/arg", "{\"Value\":\"test\"}");
                MessagePoster.Publish("Inproc", "test/full", "1");
                MessagePoster.Publish("Inproc", "test/void", "agebull");

                await MessagePoster.PublishAsync("Inproc", "async/res", "{\"Value\":\"test\"}");
                await MessagePoster.PublishAsync("Inproc", "async/arg", "{\"Value\":\"test\"}");
                await MessagePoster.PublishAsync("Inproc", "async/full", "{\"Value\":\"test\"}");
                await MessagePoster.PublishAsync("Inproc", "async/void", "{\"Value\":\"test\"}");
            }
        }
    }
}
