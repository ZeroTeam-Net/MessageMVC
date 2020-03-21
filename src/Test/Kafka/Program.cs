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
            await KafkaMessageMVC.UseKafka(typeof(Program).Assembly, false);

            _ = Task.Run(Test);

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
        static void Test()
        {
            var producer = IocHelper.Create<IMessageProducer>();
            for (int i = 0; ZeroFlowControl.CanDo && i < 10; i++)
            {
                Thread.Sleep(100);
                producer.Producer("test1", "test/res", "agebull");
                producer.Producer("test1", "test/arg", "{'Value':'test'}");
                producer.Producer("test1", "test/full", "1");
                producer.Producer("test1", "test/void", "agebull");

                producer.Producer("test1", "async/res", "{'Value':'test'}");
                producer.Producer("test1", "async/arg", "{'Value':'test'}");
                producer.Producer("test1", "async/full", "{'Value':'test'}");
                producer.Producer("test1", "async/void", "{'Value':'test'}");
            }
        }
    }
}
