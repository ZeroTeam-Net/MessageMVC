using Agebull.Common.Logging;
using CSRedis;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using static CSRedis.CSRedisClient;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// RedisMQ消息队列
    /// </summary>
    internal class CSRedisEventReceiver : MessageReceiverBase, INetEvent
    {
        /// <summary>
        /// 构造
        /// </summary>
        public CSRedisEventReceiver() : base(nameof(CSRedisEventReceiver))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(CSRedisEventReceiver);

        /// <summary>
        /// 本地代理
        /// </summary>
        private CSRedisClient client;

        /// <summary>
        /// 订阅对象
        /// </summary>
        private SubscribeObject subscribeObject;

        private TaskCompletionSource<bool> loopTask;
        private CancellationToken token;
        async Task<bool> IMessageReceiver.Loop(CancellationToken t)
        {
            token = t;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    client = new CSRedisClient(RedisOption.Instance.ConnectionString);
                    subscribeObject = client.Subscribe((Service.ServiceName, OnMessagePush));
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(() => $"CSRedisEventReceiver Loop.\r\n{ex}");
                    await Task.Delay(3000);
                }
            }
            loopTask = new TaskCompletionSource<bool>();
            await loopTask.Task;
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        Task ILifeFlow.Closing()
        {
            try
            {
                loopTask?.SetResult(true);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task ILifeFlow.Close()
        {
            try
            {
                subscribeObject?.Dispose();
                client?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            return Task.CompletedTask;
        }
        #region 消息处理

        /// <summary>
        /// 消息处理
        /// </summary>
        private void OnMessagePush(SubscribeMessageEventArgs args)
        {
            try
            {
                if (SmartSerializer.TryToMessage(args.Body, out var message))
                {
                    message.Service = Service.ServiceName;
                    MessageProcessor.OnMessagePush(Service, message, true, null).Wait();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        #endregion

    }


}
