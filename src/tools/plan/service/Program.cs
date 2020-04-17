using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tools;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var con = ConfigurationManager.Root.GetSection("MessageMVC:Redis:ConnectionString").Value;
            RedisHelper.Initialization(new CSRedisClient(con));
            ZeroFlowControl.RegistService(new ZeroService
            {
                ServiceName = "PlanManager",
                Receiver = new PlanReceiver()
            });
            var services = DependencyHelper.ServiceCollection;
            services.UseCsRedis();
            services.AddSingleton<IMessageMiddleware, ReverseProxyMiddleware>();//ͨ����������������ƻ�������Ϣ����
            services.AddSingleton<IMessagePoster, HttpPoster>();
            await services.UseFlowAsync();
        }
    }
}