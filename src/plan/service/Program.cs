using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var connectionString = ConfigurationManager.Get("Redis").GetStr("ConnectionString");
            RedisHelper.Initialization(new CSRedis.CSRedisClient(connectionString));

            ZeroFlowControl.RegistService(new ZeroService
            {
                ServiceName = "PlanManager",
                TransportBuilder = name => new PlanTransfer()
            });
            var services = IocHelper.ServiceCollection;
            services.UseCsRedis();
            services.AddSingleton<IMessageMiddleware, ReverseProxyMiddleware>();//ͨ����������������ƻ�������Ϣ����
            services.AddSingleton<IMessagePoster, HttpPoster>();
            await services.UseFlowAsync();
        }
    }
}