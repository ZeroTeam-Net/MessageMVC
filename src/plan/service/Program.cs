using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            RedisHelper.Initialization(new CSRedis.CSRedisClient(ConfigurationManager.Get("Redis").GetStr("ConnectionString")));

            ZeroFlowControl.RegistService(new ZeroService
            {
                ServiceName = "PlanManager",
                TransportBuilder = name => new PlanTransfer()
            });
            var services = IocHelper.ServiceCollection;
            services.UseCsRedis();
            services.AddSingleton<IMessageMiddleware,ReverseProxyMiddleware>();//ͨ����������������ƻ�������Ϣ����
            services.AddSingleton<IMessageProducer, HttpProducer>();
            services.UseFlowByAutoDiscory();
            Console.ReadKey();
        }
    }
}