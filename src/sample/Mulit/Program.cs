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
            DependencyHelper.ServiceCollection.AddKafka();
            DependencyHelper.ServiceCollection.AddZeroRpc();
            DependencyHelper.ServiceCollection.AddMessageMvc(typeof(Program));

            MessagePoster.Publish("test1", "test", "");
            MessagePoster.Publish("Inproc", "test", "");

            Console.ReadKey();
            Console.WriteLine("Bye bye.");
        }
    }
}
