using Agebull.Common.Ioc;
using System;
using System.Reflection;
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
        public static void UseKafka(Assembly assembly, bool waitEnd)
        {
            Console.WriteLine("Weconme ZeroTeam KafkaMVC");
            IocHelper.AddTransient<IAppMiddleware, ZeroGlobal>();
            IocHelper.AddTransient<IAppMiddleware, AddInImporter>();
            IocHelper.AddTransient<IMessageConsumer, KafkaConsumer>();
            IocHelper.AddSingleton<IMessageMiddleware, ApiExecuter>();

            ZeroApplication.CheckOption();
            KafkaProducer.Initialize();
            ZeroApplication.Discove(assembly);
            ZeroApplication.Initialize();
            if (waitEnd)
                ZeroApplication.RunAwaite();
            else
                _ = ZeroApplication.RunAsync();
        }
    }
}
