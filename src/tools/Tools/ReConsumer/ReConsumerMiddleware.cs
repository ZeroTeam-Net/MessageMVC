using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
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

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppOption config)
        {
            path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "message");
        }

        private string path;
        private List<string> files;
        /// <summary>
        /// 开启
        /// </summary>
        void IFlowMiddleware.Start()
        {
            files = IOHelper.GetAllFiles(path, "*.msg");
            if (files.Count > 0)
            {
                _ = ReConsumer();
            }
        }

        /// <summary>
        ///     重新消费错误消息
        /// </summary>
        private async Task ReConsumer()
        {
            var service = new ZeroService
            {
                Receiver = new EmptyReceiver()
            };
            await Task.Yield();
            foreach (var file in files)
            {
                if (!ZeroFlowControl.IsRuning)
                {
                    return;
                }

                await Task.Delay(10);
                try
                {
                    var json = File.ReadAllText(file);
                    if (SmartSerializer.TryToMessage(json, out var message))
                    {
                        service.ServiceName = message.ServiceName;
                        await MessageProcessor.OnMessagePush(service, message, true, null);
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "异常消息重新处理出错");
                }
            }
        }
    }
}
