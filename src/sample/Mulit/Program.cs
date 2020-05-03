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
        static void Main()
        {
            DependencyHelper.AddTransient<IFlowMiddleware, ZeroRpcFlow>();
            DependencyHelper.AddSingleton<IServiceReceiver, ZeroRpcReceiver>();

            DependencyHelper.ServiceCollection.UseKafka();
            DependencyHelper.ServiceCollection.UseFlow(typeof(Program));

            MessagePoster.Publish("test1", "test", "");
            MessagePoster.Publish("Inproc", "test", "");

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
