using Agebull.Common;
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
    /// RedisMQ消费者
    /// </summary>
    internal class CSRedisQueue : MessageReceiverBase, IMessageConsumer
    {
        /// <summary>
        /// 构造
        /// </summary>
        public CSRedisQueue() : base(nameof(CSRedisQueue))
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
        private string jobList;
        private string errList;
        private string bakList;

        /// <summary>
        /// 初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(CSRedisQueue));


            jobList = $"msg:{Service.ServiceName}";
            bakList = $"bak:{Service.ServiceName}";
            errList = $"err:{Service.ServiceName}";

            //启动异常守护
            Task.Factory.StartNew(Guard, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }

        private TaskCompletionSource<bool> loopTask;
        private CancellationToken token;
        async Task<bool> IMessageReceiver.Loop(CancellationToken t)
        {
            token = t;
            while (!t.IsCancellationRequested)
            {
                try
                {
                    client = new CSRedisClient(RedisOption.Instance.ConnectionString);
                    if (client == null)
                        continue;
                    subscribeObject = client.Subscribe((Service.ServiceName, arg => OnMessagePush()));
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error(() => $"启动订阅失败.{ex.Message}");
                    await Task.Delay(3000);
                }
            }
            OnMessagePush();
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
            //try
            //{
            //    if (!subscribeObject.IsUnsubscribed)
            //        subscribeObject.Unsubscribe();
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
            //subscribeObject?.Dispose();
            client?.Dispose();
            return Task.CompletedTask;
        }
        #region 消息处理

        /// <summary>
        /// 异常守护
        /// </summary>
        private async Task Guard()
        {
            logger.Information("异常消息守卫已启动");
            using var client = new CSRedisClient(RedisOption.Instance.ConnectionString);
            //处理错误重新入列
            while (true)
            {
                var key = client.LPop(errList);
                if (string.IsNullOrEmpty(key))
                {
                    break;
                }

                logger.Debug(() => $"异常消息重新入列:{key}");
                client.LPush(jobList, key);
            }
            //非正常处理还原
            while (ZeroFlowControl.IsAlive)
            {
                await Task.Delay(RedisOption.Instance.GuardCheckTime, token);
                try
                {
                    var key = await client.LPopAsync(bakList);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    var guard = $"guard:{Service.ServiceName}:{key}";
                    if (await client.SetNxAsync(guard, "Guard"))
                    {
                        client.Expire(guard, RedisOption.Instance.MessageLockTime);
                        logger.Debug(() => $"超时消息重新入列:{key}");
                        await client.LPushAsync(jobList, key);
                        await client.DelAsync(guard);
                    }
                }
                catch (Exception ex)
                {
                    logger.Information(() => $"异常消息守卫错误.{ex.Message }");
                }
            }
        }

        /// <summary>
        /// 单处理守卫变量
        /// </summary>
        private int isBusy = 0;

        /// <summary>
        /// 消息处理
        /// </summary>
        private void OnMessagePush()
        {
            try
            {
                //仅允许一个处理程序在运行
                if (Interlocked.Increment(ref isBusy) != 1)
                {
                    return;
                }

                _ = ReadList();
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
        private async Task ReadList()
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!await Read())
                        return;
                }
                catch (RedisClientException ex)
                {
                    logger.Exception(ex);
                    logger.Warning(() => $"ReadList error.{ex.Message }");
                    await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    logger.Exception(ex);
                    logger.Warning(() => $"ReadList error.{ex.Message }");
                    await Task.Delay(300);
                }
            }
        }


        /// <summary>
        /// 取出队列,全部处理,为空后退出
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Read()
        {
            var id = client.RPopLPush(jobList, bakList);
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var key = $"msg:{Service.ServiceName}:{id}";
            var guard = $"guard:{Service.ServiceName}:{id}";
            if (!await client.SetNxAsync(guard, "Guard"))
            {
                await Task.Delay(RedisOption.Instance.MessageLockTime);
                return true;
            }
            client.Expire(guard, RedisOption.Instance.MessageLockTime);

            var str = client.Get(key);
            if (string.IsNullOrEmpty(str))
            {
                logger.Warning(() => $"ReadList key empty.{key}");
                await client.DelAsync(key);
                await client.LRemAsync(bakList, 0, id);
                await client.DelAsync(guard);
                return true;
            }
            IInlineMessage item;
            try
            {
                item = SmartSerializer.ToMessage(str);
            }
            catch (Exception ex)
            {
                logger.Warning(() => $"ReadList deserialize error.{ex.Message }.{key} =>{str}");
                await client.DelAsync(key);
                await client.LRemAsync(bakList, 0, id);
                await client.DelAsync(guard);
                return true;
            }
            //item.Trace ??= TraceInfo.New(item.ID);
            item.Topic = Service.ServiceName;

            _ = MessageProcessor.OnMessagePush(Service, item, true, null);

            return true;
        }
        async Task<bool> CheckState(MessageState state, string key, string id, string guard)
        {
            try
            {
                switch (state)
                {
                    case MessageState.AsyncQueue:
                    case MessageState.Success:
                    case MessageState.FormalError:
                        await client.DelAsync(key);
                        break;
                    case MessageState.Deny:
                    case MessageState.Failed:
                        if (RedisOption.Instance.FailedIsError)
                            await client.LPushAsync(errList, id);
                        else
                            await client.DelAsync(key);
                        break;
                    case MessageState.Unhandled:
                    case MessageState.NonSupport:
                        if (RedisOption.Instance.NoSupperIsError)
                            await client.LPushAsync(errList, id);
                        else
                            await client.DelAsync(key);
                        break;
                    default:
                        await client.LPushAsync(errList, id);
                        break;
                }
                await client.LRemAsync(bakList, 0, id);
                await client.DelAsync(guard);
                return true;
            }
            catch (RedisClientException ex)
            {
                logger.Warning(() => $"同步状态失败.{ex.Message }");
                return false;
            }
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否需要发送回执</returns>
        async Task<bool> IMessageReceiver.OnResult(IInlineMessage item, object tag)
        {
            if(item.Trace.Context != null)
                item.Trace.Context.Option["Receipt"] = "false";
            var key = $"msg:{Service.ServiceName}:{item.ID}";
            var guard = $"guard:{Service.ServiceName}:{item.ID}";
            while (!await CheckState(item.State, key, item.ID, guard))
            {
                await Task.Delay(3000);
            }
            return true;
        }

        #endregion
    }
}
