using Agebull.Common.Ioc;
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
            RedisHelper.Initialization(new CSRedis.CSRedisClient(RedisOption.Instance.ConnectionString));

            ZeroFlowControl.RegistService(new ZeroService
            {
                ServiceName = "PlanManager",
                Receiver = new PlanReceiver()
            });
            var services = IocHelper.ServiceCollection;
            services.UseCsRedis();
            services.AddSingleton<IMessageMiddleware, ReverseProxyMiddleware>();//ͨ����������������ƻ�������Ϣ����
            services.AddSingleton<IMessagePoster, HttpPoster>();
            await services.UseFlowAsync();
        }
    }
}