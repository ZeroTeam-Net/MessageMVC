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
        static async Task Main(string[] args)
        {

            Console.WriteLine("Weconme ZeroTeam MessageMVC");

            ThreadPool.GetMaxThreads(out var worker, out _);
            ThreadPool.SetMaxThreads(worker, 4096);
            //ThreadPool.GetAvailableThreads(out worker, out io);

            KafkaMessageMVC.UseKafka();

            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(Program).Assembly);

            ZeroApplication.Initialize();

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Factory.StartNew(Test);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

            await ZeroApplication.RunAwaiteAsync();

            ZeroTrace.SystemLog("Bye bye.");
            /*//
            ZeroApplication.Run();
            Console.WriteLine("Runint");
            Console.ReadKey();
            ZeroApplication.Shutdown();
            Console.WriteLine("Bye bye");
            Console.ReadKey();*/
        }
        static void Test()
        {
            Thread.Sleep(3000);
            var producer = IocHelper.Create<IMessageProducer>();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(100);
                producer.Producer("test1", "api/test", "{}");
            }
        }
    }
}
