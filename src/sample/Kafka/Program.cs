using Agebull.Common.Ioc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            //DependencyHelper.Flush();
            var services = DependencyHelper.ServiceCollection;
            services.AddMessageMvcKafka();
            services.AddMessageMvc(typeof(Program).Assembly);
            //自动退出
            await services.UseMessageMvc();
        }
        static async void Test()
        {
            //for (int i = 0; ZeroAppOption.Instance.IsRuning && i < 10; i++)
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
