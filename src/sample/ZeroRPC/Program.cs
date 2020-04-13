using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            DependencyHelper.AddSingleton<IFlowMiddleware, ZeroRpcFlow>();
            DependencyHelper.AddSingleton<IFlowMiddleware>(ZeroPostProxy.Instance);
            DependencyHelper.AddSingleton<IServiceTransfer, ZeroRpcReceiver>();
            DependencyHelper.AddSingleton<IMessagePoster, ZeroRPCPoster>();


            await DependencyHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            //MessagePoster.Producer("test1", "test", "");
            //MessagePoster.Producer("Inproc", "test", "");

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
