using Agebull.Common.Ioc;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            DependencyHelper.ServiceCollection.BindingMessageMvc();
            DependencyHelper.ServiceCollection.AddMessageMvcKafka();
            DependencyHelper.ServiceCollection.AddMessageMvcZeroRpc();
            DependencyHelper.ServiceCollection.AddMessageMvc();
            await DependencyHelper.ServiceCollection.UseMessageMvc();
            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
