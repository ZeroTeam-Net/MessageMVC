using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.RedisMQ;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            IocHelper.AddSingleton<IFlowMiddleware, PlanManager>();
            var services = IocHelper.ServiceCollection;
            RedisHelper.Initialization(new CSRedis.CSRedisClient(ConfigurationManager.Get("Redis").GetStr("ConnectionString")));
            services.UseCsRedis();
            services.AddSingleton<IMessageProducer, HttpProducer>();
            services.UseFlowByAutoDiscory();
            Console.ReadKey();
        }
    }
}