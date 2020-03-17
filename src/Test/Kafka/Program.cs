using Agebull.Common.Ioc;
using KafkaTest;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main(string[] args)
        {

            // KafkaConsumerDemo.Start();
            IocHelper.AddTransient<IMessageConsumer, KafkaConsumer>();
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(Program).Assembly);
            ZeroTeam.MessageMVC.Kafka.KafkaProducer.Initialize();
            ZeroApplication.Initialize();
            IocHelper.Create<IMessageProducer>().Producer("test1", "api/test", "{}");
            await ZeroApplication.RunAwaiteAsync();

            ZeroTrace.SystemLog("Bye bye.");
            /*//Task.Factory.StartNew(Test);
            ZeroApplication.Run();
            Console.WriteLine("Runint");
            Console.ReadKey();
            ZeroApplication.Shutdown();
            Console.WriteLine("Bye bye");
            Console.ReadKey();*/
        }
        static void Test()
        {
            var producer = IocHelper.Create<IMessageProducer>();
            while (true)
            {
                for(int i=0;i<10;i++)
                    producer.Producer("test1", "api/test", "{}");
                Thread.Sleep(1);
            }
        }
    }
}
