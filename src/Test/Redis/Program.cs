using Agebull.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.RedisMQ;
using ZeroTeam.MessageMVC.RedisMQ.Sample.Controler;
using ZeroTeam.MessageMVC.Sample;

namespace MicroZero.Kafka.QueueStation
{
    class Program
    {
        static async Task Main()
        {
            var service = IocHelper.ServiceCollection;
            service.UseCsRedis();
            await service.UseFlow(typeof(Program).Assembly, false);

            for (int i = 1; i <= 100; i++)
            {
                MessageProducer.Producer("AppEvent", "v1/error", new AppErrorInfo
                {
                    AppName = ZeroFlowControl.AppName,
                    Module = nameof(Program),
                    Level = "Error",
                    Message = "Test"
                });
                MessageProducer.Producer("OrderEvent", "offline/v1/new", new UnionOrder
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
            Console.ReadKey();
            //ZeroFlowControl.Shutdown();
            Console.WriteLine("Bye bye.");
        }
    }
}
