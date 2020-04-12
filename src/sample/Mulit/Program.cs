using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            IocHelper.AddTransient<IFlowMiddleware, ZeroRpcFlow>();
            IocHelper.AddSingleton<IServiceTransfer, ZeroRpcReceiver>();

            IocHelper.ServiceCollection.UseKafka();
            await IocHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            MessagePoster.Publish("test1", "test", "");
            MessagePoster.Publish("Inproc", "test", "");

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
