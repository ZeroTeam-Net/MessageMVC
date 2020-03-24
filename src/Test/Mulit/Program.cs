using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            IocHelper.AddTransient<IFlowMiddleware, MicroZeroApplication>();
            IocHelper.AddSingleton<IRpcTransfer, ZmqRpcTransport>();
            //IocHelper.AddTransient<IP, MicroZeroApplication>();

            //IocHelper.ServiceCollection.UseKafka();
            //IocHelper.ServiceCollection.UseZeroMQInporc();
            await IocHelper.ServiceCollection.UseFlow(typeof(Program).Assembly, false);

            //MessageProducer.Producer("test1", "test", "");
            //MessageProducer.Producer("Inproc", "test", "");

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
