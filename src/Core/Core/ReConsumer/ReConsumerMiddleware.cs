using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 重新消费未正确处理消息的中间件
    /// </summary>
    public class ReConsumerMiddleware : IFlowMiddleware
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string IFlowMiddleware.RealName => "ReConsumerMiddleware";

        /// <summary>
        /// 等级
        /// </summary>
        int IFlowMiddleware.Level => 0;

        /// <summary>
        ///     关闭
        /// </summary>
        void IFlowMiddleware.Close()
        {
        }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppConfigRuntime config)
        {
            path = IOHelper.CheckPath(ZeroFlowControl.Config.DataFolder, "message");
        }
        string path;
        List<string> files;
        /// <summary>
        /// 开启
        /// </summary>
        void IFlowMiddleware.Start()
        {
            files = IOHelper.GetAllFiles(path, "*.msg");
            if (files.Count > 0)
                _ = ReConsumer();
        }

        /// <summary>
        ///     重新消费错误消息
        /// </summary>
        async Task ReConsumer()
        {
            var service = new ZeroService();
            await Task.Yield();
            foreach (var file in files)
            {
                if (!ZeroFlowControl.CanDo)
                    return;
                Thread.Sleep(10);
                try
                {
                    var json = File.ReadAllText(file);
                    var item = JsonHelper.DeserializeObject<MessageItem>(json);
                    service.ServiceName = item.ServiceName;
                    await MessageProcess.OnMessagePush(service, item);
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "ReProducerMiddleware.ReProducer");
                }
            }
        }
    }
}
