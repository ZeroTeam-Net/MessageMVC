using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            IocHelper.AddSingleton<IFlowMiddleware, ZeroRpcFlow>();
            IocHelper.AddSingleton<IFlowMiddleware, ZeroPostProxy>();
            IocHelper.AddSingleton<IServiceTransfer, ZeroRpcTransport>();
            IocHelper.AddSingleton<IMessagePoster, ZeroRPCPoster>();


            await IocHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            //MessagePoster.Producer("test1", "test", "");
            //MessagePoster.Producer("Inproc", "test", "");

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
