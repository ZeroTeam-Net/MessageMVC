using Agebull.Common.Ioc;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroMQ.Inporc;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            await ZeroMQInporc.UseZeroMQ(typeof(Program).Assembly, false);

            await Test();

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
        static async Task Test()
        {
            var producer = IocHelper.Create<IMessageProducer>();
            for (int i = 0; ZeroFlowControl.CanDo && i < 10; i++)
            {
                Thread.Sleep(100);
                producer.Producer("test1", "test/res", "agebull");
                producer.Producer("test1", "test/arg", "{\"Value\":\"test\"}");
                producer.Producer("test1", "test/full", "1");
                producer.Producer("test1", "test/void", "agebull");

                await producer.ProducerAsync("test1", "async/res", "{\"Value\":\"test\"}");
                await producer.ProducerAsync("test1", "async/arg", "{\"Value\":\"test\"}");
                await producer.ProducerAsync("test1", "async/full", "{\"Value\":\"test\"}");
                await producer.ProducerAsync("test1", "async/void", "{\"Value\":\"test\"}");
            }
        }
    }
}
