using Agebull.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.Sample;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            var service = DependencyHelper.ServiceCollection;
            service.BindingMessageMvc();
            service.AddMessageMvcRedis();
            service.AddMessageMvc();
            _ = Task.Run(Test);
            await service.UseMessageMvc();
            //ZeroFlowControl.Shutdown();
            Console.WriteLine("Bye bye.");
        }

        private static void Test()
        {
            for (int i = 1; i <= 100; i++)
            {
                MessagePoster.Publish("OrderEvent", "offline/v1/new", new UnionOrder
                {
                    Items = new List<UnionOrderItem>
                    {
                        new UnionOrderItem
                        {
                            ItemType = SkuType.GeneralProduct,
                            SkuName = "女鞋E思Q",
                            SalePrice = 50,
                            Number = 3,
                            Amount = 500,
                            Pay = 100,
                            BaseSkuSid = 878465627947009L
                        }
                    },
                    Amount = 500,
                    Pay = 500
                });
            }
        }
    }
}
