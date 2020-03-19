using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    public static class KafkaMessageMVC
    {
        public static void UseKafka()
        {
            IocHelper.AddTransient<IAppMiddleware, ZeroGlobal>();
            IocHelper.AddTransient<IAppMiddleware, AddInImporter>();
            IocHelper.AddTransient<IMessageConsumer, KafkaConsumer>();
            IocHelper.AddSingleton<IMessageMiddleware, ApiExecuter>();
            KafkaProducer.Initialize();
        }
    }
}
