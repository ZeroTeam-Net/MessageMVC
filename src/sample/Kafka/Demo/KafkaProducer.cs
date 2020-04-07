using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace KafkaTest
{
    /// <summary>
    /// Kafka消息生产者
    /// </summary>
    public class KafkaPoster
    {

        private static readonly ProducerConfig conf = new ProducerConfig { BootstrapServers = "47.111.0.73:9092" };
        private static readonly string pushTopic = "test1";
        private static IProducer<Null, string> p = new ProducerBuilder<Null, string>(conf).Build();
        private static int num = 0;
        private static async Task ProduceAsync(string str)
        {
            try
            {
                var ret = await p.ProduceAsync(pushTopic, new Message<Null, string> { Value = str });
                var flag = ret.Status == PersistenceStatus.Persisted;

                Console.WriteLine($"Production<{num++}>:{str}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Production<Error>:{e}");
            }
        }

        public static async Task Start()
        {
            for (var i = 0; i < 10; i++)
            {
                await ProduceAsync($"DEMO {i}");
                await Task.Delay(1000);
            }
            Console.WriteLine("Production Close");
        }
    }
}
