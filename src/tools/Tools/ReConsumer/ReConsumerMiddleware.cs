using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 重新消费未正确处理消息的中间件
    /// </summary>
    public class ReConsumerMiddleware : IFlowMiddleware
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(ReConsumerMiddleware);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Framework;

        Task IZeroDiscover.Discovery()
        {
            path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "message");
            return Task.CompletedTask;
        }

        private string path;

        /// <summary>
        /// 启动
        /// </summary>
        async Task ILifeFlow.Open()
        {
            var files = IOHelper.GetAllFiles(path, "*.msg");
            if (files.Count == 0)
            {
                return;
            }
            ILogger logger = DependencyHelper.LoggerFactory.CreateLogger<ReConsumerMiddleware>();
            logger.Information($"重新消费错误消息.共{files.Count}个");
            var service = new ZeroService
            {
                Receiver = new EmptyReceiver()
            };
            await Task.Yield();
            foreach (var file in files)
            {
                if (!ZeroAppOption.Instance.IsRuning)
                {
                    return;
                }

                await Task.Delay(10);
                try
                {
                    var json = File.ReadAllText(file);
                    if (SmartSerializer.TryToMessage(json, out var message))
                    {
                        service.ServiceName = message.Service;
                        await MessageProcessor.OnMessagePush(service, message, true, null);
                    }
                }
                catch (Exception e)
                {
                    logger.Exception(e, "异常消息重新处理出错");
                }
            }
        }
    }
}
