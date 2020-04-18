using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using CSRedis;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using static CSRedis.CSRedisClient;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// RedisMQ消息队列
    /// </summary>
    internal class CSRedisConsumer : MessageReceiverBase, INetEvent
    {
        /// <summary>
        /// 构造
        /// </summary>
        public CSRedisConsumer() : base(nameof(CSRedisConsumer))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(CsRedisPoster);

        ILogger logger;

        /// <summary>
        /// 本地代理
        /// </summary>
        private CSRedisClient client;

        /// <summary>
        /// 订阅对象
        /// </summary>
        private SubscribeObject subscribeObject;
        /// <summary>
        /// 初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(CSRedisConsumer));
        }

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
                    if (client == null)
                        continue;
                    subscribeObject = client.Subscribe((Service.ServiceName, OnMessagePush));
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error(() => $"启动订阅失败.{ex.Message}");
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
        async Task IMessageReceiver.Close()
        {
            try
            {
                subscribeObject?.Unsubscribe();
                while (isBusy > 0)//等处理线程退出
                {
                    await Task.Delay(10);
                }

                loopTask?.SetResult(true);
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
            subscribeObject?.Dispose();
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
        private void OnMessagePush(SubscribeMessageEventArgs args)
        {
            try
            {
                //仅允许一个处理程序在运行
                if (Interlocked.Increment(ref isBusy) != 1)
                {
                    return;
                }

                _ = ReadList(args.Body);
            }
            finally
            {
                Interlocked.Decrement(ref isBusy);
            }
        }

        #endregion

        #region ReadList

        /// <summary>
        /// 取出队列,全部处理,为空后退出
        /// </summary>
        /// <returns></returns>
        private async Task ReadList(string id)
        {
            await Task.Delay(300);
            try
            {
                if (await Read(id))
                    return;
            }
            catch (RedisClientException ex)
            {
                logger.Warning(() => $"ReadList error.{ex.Message }");
            }
            catch (Exception ex)
            {
                var exxx = ex;
                logger.Warning(() => $"ReadList error.{ex.Message }");
            }
            _ = ReadList(id);
        }


        /// <summary>
        /// 取出队列,全部处理,为空后退出
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Read(string id)
        {
            var key = $"msg:{Service.ServiceName}:{id}";
            var str = client.Get(key);
            if (string.IsNullOrEmpty(str))
            {
                logger.Warning(() => $"ReadList key empty.{key}");
                await client.DelAsync(key);
                return true;
            }
            try
            {
                if (SmartSerializer.TryToMessage(str, out var message))
                {
                    message.Trace ??= TraceInfo.New(message.ID);
                    message.Trace.TraceId = id;
                    message.Topic = Service.ServiceName;
                    await MessageProcessor.OnMessagePush(Service, message, true, null);//BUG:应该配置化同步或异步}
                }
                else
                {
                    logger.Warning(() => $"列表({key})内容错误,自动删除. =>{str}");
                    await client.DelAsync(key);
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
            return true;
        }

        #endregion
    }
}
