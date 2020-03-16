using Agebull.Common.Ioc;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Kafka;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IocHelper.AddTransient<IMessageConsumer, KafkaConsumer>();
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(Program).Assembly);
            MessageProducer.Initialize();
            ZeroApplication.Initialize();
            MessageProducer.Publish("test1", "api/test","{}");
            await ZeroApplication.RunAwaiteAsync();
        }
    }
}
