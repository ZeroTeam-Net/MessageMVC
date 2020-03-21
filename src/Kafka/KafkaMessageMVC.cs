using Agebull.Common.Ioc;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    /// KafkaMvc
    /// </summary>
    public static class KafkaMessageMVC
    {
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="waitEnd"></param>
        public static Task UseKafka(Assembly assembly, bool waitEnd)
        {
            Console.WriteLine("Weconme ZeroTeam KafkaMVC");

            IocHelper.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取
            IocHelper.AddTransient<IFlowMiddleware, AddInImporter>();//插件载入
            IocHelper.AddTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
            IocHelper.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            IocHelper.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            IocHelper.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行

            //消息存储与异常消息重新消费
            IocHelper.AddTransient<IMessageMiddleware, StorageMiddleware>();
            IocHelper.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();
            //主流程
            ZeroFlowControl.CheckOption();
            KafkaProducer.Initialize();
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Initialize();
            if (waitEnd)
               return ZeroFlowControl.RunAwaiteAsync();
            else
                return ZeroFlowControl.RunAsync();
        }
    }
}
