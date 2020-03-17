using Agebull.Common.Configuration;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace KafkaTest
{
    public class KafkaConsumerDemo
    {

        private static readonly string bootstrapServers = "47.111.0.73:9092";
        private static readonly string pollTopic = "test1";
        public static void Start()
        {
            try
            {
                var conf = new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = "kaisen",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
                {
                    c.Subscribe(pollTopic);
                    CancellationTokenSource cts = new CancellationTokenSource();
                    while (true)
                    {
                        var cr = c.Consume(cts.Token);
                        if (cr == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        string xmlStr = cr.Value;
                        c.Commit();
                        Console.WriteLine($"Consumer:{xmlStr}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Consumer<Error>:{ex}");
            }
        }
    }
}
