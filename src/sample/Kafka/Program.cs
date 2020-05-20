using Agebull.Common.Ioc;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            var services = DependencyHelper.ServiceCollection;
            services.UseKafka();
            await services.UseFlowAndWait(typeof(Program));
        }
        static async void Test()
        {
            //for (int i = 0; ZeroFlowControl.IsRuning && i < 10; i++)
            {
                await Task.Delay(100);

                //await MessagePoster.PublishAsync("test1", "test/res", "agebull");
                //await MessagePoster.PublishAsync("test1", "test/arg", "{'Value':'test'}");
                await MessagePoster.PublishAsync("test1", "test/full", "{'name':'test'}");
                //await MessagePoster.PublishAsync("test1", "test/void", "agebull");

                //await MessagePoster.PublishAsync("test1", "async/res", "{'Value':'test'}");
                //await MessagePoster.PublishAsync("test1", "async/arg", "{'Value':'test'}");
                //await MessagePoster.PublishAsync("test1", "async/full", "{'Value':'test'}");
                //await MessagePoster.PublishAsync("test1", "async/void", "{'Value':'test'}");
            }
        }
    }
}
