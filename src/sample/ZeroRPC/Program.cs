using Agebull.Common.Ioc;
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
            DependencyHelper.AddSingleton<IMessagePoster>(ZeroRPCPoster.Instance);
            DependencyHelper.AddTransient<IServiceReceiver, ZeroRpcReceiver>();
            DependencyHelper.AddSingleton<INetEvent, ZeroEventReceiver>();


            await DependencyHelper.ServiceCollection.UseMessageMvc();
        }
    }
}
