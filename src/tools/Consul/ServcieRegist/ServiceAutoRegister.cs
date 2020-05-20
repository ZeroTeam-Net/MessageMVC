using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Consul;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    /// Consul注册流程控制
    /// </summary>
    internal class ServiceAutoRegister : IFlowMiddleware
    {
        ///<inheritdoc/>
        public int Level => MiddlewareLevel.Last;

        ///<inheritdoc/>
        public string Name => nameof(ServiceAutoRegister);

        List<AgentServiceRegistration> services;

        ConsulClient consulClient;
        ILogger logger;
        Task ILifeFlow.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ServiceAutoRegister));
            return Task.CompletedTask;
        }
        async Task ILifeFlow.Open()
        {
            //请求注册的 Consul 地址
            var consulUrl = $"http://{ConsulOption.Instance.ConsulIP}:{ConsulOption.Instance.ConsulPort}";
            consulClient = new ConsulClient(x =>
            {
                x.Address = new Uri(consulUrl);
            });
            logger.Information(consulUrl);
            services = new List<AgentServiceRegistration>();
            //添加 urlprefix-/servicename 格式的 tag 标签，以便 Fabio 识别
            foreach (var service in ZeroFlowControl.Services.Values.ToArray())
            {
                if (!(service.Receiver is IServiceReceiver))
                {
                    continue;
                }
                var reg = new AgentServiceRegistration()
                {
                    Checks = new[] { ConsulOption.Instance.AgentServiceCheck },
                    ID = $"{ConsulOption.Instance.IP}_{ConsulOption.Instance.Port}_{service.ServiceName}",
                    Name = service.ServiceName,
                    Address = ConsulOption.Instance.IP,
                    Port = ConsulOption.Instance.Port,
                    Tags = new[] { $"urlprefix-/{service.ServiceName}" }
                };
                while (true)
                {
                    var res = await consulClient.Agent.ServiceRegister(reg);
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                        break;
                    await Task.Delay(1000);
                    logger.Information(() => $"[{service.ServiceName}]注册失败：{res.StatusCode}");
                }

                logger.Information(() => $"[{service.ServiceName}]注册成功");

                services.Add(reg);
            }
        }

        async Task ILifeFlow.Close()
        {
            foreach (var ser in services)
            {
                var res = await consulClient.Agent.ServiceDeregister(ser.ID);//服务停止时取消注册
                logger.Information(() => $"服务[{ser.Name}]反注册到Consul：{res.StatusCode}");
            }
            consulClient.Dispose();
        }
    }
}