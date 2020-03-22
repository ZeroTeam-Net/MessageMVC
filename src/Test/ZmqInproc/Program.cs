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
            IocHelper.ServiceCollection.UseZeroMQInporc();
            await IocHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            await Test();

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
        static async Task Test()
        {
            //var MessageProducer = IocHelper.Create<IMessageProducer>();
            for (int i = 0; ZeroFlowControl.CanDo && i < 10; i++)
            {
                Thread.Sleep(100);
                MessageProducer.Producer("Inproc", "test/res", "agebull");
                MessageProducer.Producer("Inproc", "test/arg", "{\"Value\":\"test\"}");
                MessageProducer.Producer("Inproc", "test/full", "1");
                MessageProducer.Producer("Inproc", "test/void", "agebull");

                await MessageProducer.ProducerAsync("Inproc", "async/res", "{\"Value\":\"test\"}");
                await MessageProducer.ProducerAsync("Inproc", "async/arg", "{\"Value\":\"test\"}");
                await MessageProducer.ProducerAsync("Inproc", "async/full", "{\"Value\":\"test\"}");
                await MessageProducer.ProducerAsync("Inproc", "async/void", "{\"Value\":\"test\"}");
            }
        }
    }
}
