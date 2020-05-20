using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Consul;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    /// Consul事件订阅
    /// </summary>
    internal class ConsulEventConsumer : MessageReceiverBase, INetEvent
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ConsulEventConsumer() : base(nameof(ConsulEventConsumer))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(ConsulEventPoster);

        ILogger logger;

        /// <summary>
        /// 本地代理
        /// </summary>
        private ConsulClient client;


        /// <summary>
        /// 初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ConsulEventConsumer));
        }

        private CancellationToken token;

        async Task<bool> IMessageReceiver.Loop(CancellationToken t)
        {
            token = t;
            var consulUrl = $"http://{ConsulOption.Instance.ConsulIP}:{ConsulOption.Instance.ConsulPort}";
            client = new ConsulClient(x =>
            {
                x.Address = new Uri(consulUrl);
            });
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "consul");
            var file = Path.Combine(path, $"{Service.ServiceName}.num");
            string id = null;
            if (File.Exists(file))
            {
                id = await File.ReadAllTextAsync(file);
            }
            //var query = new QueryOptions
            //{
            //    Consistency = QueryOptions.Default.Consistency,
            //    Datacenter = QueryOptions.Default.Datacenter,
            //    Near = QueryOptions.Default.Near,
            //    Token = QueryOptions.Default.Token,
            //    WaitTime = TimeSpan.FromSeconds(3)
            //};
            ulong lastId = 0UL;
            if (!string.IsNullOrEmpty(id))
                ulong.TryParse(id,out lastId);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var res = await client.Event.List(Service.ServiceName, t);
                    if (res.StatusCode != System.Net.HttpStatusCode.OK || res.LastIndex== lastId)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                    lastId = res.LastIndex;
                    foreach (var ev in res.Response)
                    {
                        await OnMessagePush(ev);
                    }
                    await File.WriteAllTextAsync(file, lastId.ToString());
                }
                catch (OperationCanceledException)
                {
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error(() => $"订阅失败.{ex.Message}");
                    await Task.Delay(3000);
                }
            }
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        async Task IMessageReceiver.Close()
        {
            //try
            //{
            //    subscribeObject?.Unsubscribe();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(() => $"LoopBegin error.{ex.Message}");
            //}

            try
            {
                client?.Dispose();
                while (isBusy > 0)//等处理线程退出
                {
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                logger.Error(() => $"LoopBegin error.{ex.Message}");
            }
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.LoopComplete()
        {
            client?.Dispose();
            return Task.CompletedTask;
        }
        #region 消息处理

        /// <summary>
        /// 单处理守卫变量
        /// </summary>
        private int isBusy = 0;

        /// <summary>
        /// 消息处理
        /// </summary>
        private async Task OnMessagePush(UserEvent args)
        {
            Interlocked.Increment(ref isBusy);
            try
            {
                if (args.Payload == null)
                    return;
                var json = args.Payload.FromUtf8Bytes();
                if (SmartSerializer.TryToMessage(json, out var message))
                {
                    message.ID = args.ID;
                    message.Topic = Service.ServiceName;
                    await MessageProcessor.OnMessagePush(Service, message, true, null);
                }
            }
            finally
            {
                Interlocked.Decrement(ref isBusy);
            }
        }

        #endregion

    }
}
