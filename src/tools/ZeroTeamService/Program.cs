using Agebull.Common.Configuration;
using Agebull.EntityModel.BusinessLogic;
using BeetleX.FastHttpApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ModelApi;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Tcp;

namespace ZeroTeam.MessageMVC.Services
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddConfiguration(ConfigurationHelper.Root.GetSection("Logging")))
                .UseMessageMVC(false, services =>
                 {
                     services.AddMessageMvcRedis();
                     services.AddMessageMvcKafka();
                     services.AddMessageMvcHttpClient();
                     services.AddMessageMvcTcp();
                     services.AddMessageMvcFastHttpApi();
                     services.AddScoped<IBusinessContext, BusinessContext>();
                     services.AddTransient<IMessageMiddleware, BusinessExceptionMiddleware>();
                 });
        }
    }
}
