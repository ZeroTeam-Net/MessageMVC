using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Sample;

namespace MicroZero.Kafka.QueueStation
{
    public class TestHost : IHostedService
    {
        ILogger _logger;
        public TestHost(ILogger<TestHost> logger)
        {
            _logger = logger;
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                FlowTracer.BeginMonitor("OrderEvent");
                await MessagePoster.PublishAsync("OrderEvent", "offline/v1/new", new UnionOrder
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
                _logger.TraceMonitor(FlowTracer.EndMonitor());
                await Task.Delay(1000);
            }
        }
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
