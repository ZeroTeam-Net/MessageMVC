using Agebull.Common.Ioc;
using System;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static void Main()
        {
            DependencyHelper.ServiceCollection.AddMessageMvcKafka();
            DependencyHelper.ServiceCollection.AddMessageMvcZeroRpc();
            DependencyHelper.ServiceCollection.AddMessageMvc(typeof(Program));

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
