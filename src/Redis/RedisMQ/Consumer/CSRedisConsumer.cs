using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using CSRedis;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;
using static CSRedis.CSRedisClient;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// 表示进程内通讯
    /// </summary>
    public class CSRedisConsumer : IMessageConsumer, INetEvent
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务
        /// </summary>
        public IService Service { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <example>
        /// $"{Address}:{Port},password={PassWord},defaultDatabase={db},poolsize=50,ssl=false,writeBuffer=10240";
        /// </example>
        private RedisOption Option;

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
        public void Initialize()
        {
            Name = "RedisMQ";
            Option = ConfigurationManager.Get<RedisOption>("Redis");
            if (Option.GuardCheckTime <= 0)
            {
                Option.GuardCheckTime = 3000;
            }

            if (Option.MessageLockTime <= 0)
            {
                Option.MessageLockTime = 1000;
            }

            jobList = $"msg:{Service.ServiceName}";
            bakList = $"bak:{Service.ServiceName}";
            errList = $"err:{Service.ServiceName}";

            //启动异常守护
            Task.Factory.StartNew(Guard, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }


        void IDisposable.Dispose()
        {
        }

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        bool INetTransfer.LoopBegin()
        {
            client = new CSRedisClient(Option.ConnectionString);
            return true;
        }

        private TaskCompletionSource<bool> loopTask;
        private CancellationToken token;
        async Task<bool> INetTransfer.Loop(CancellationToken t)
        {
            token = t;
            OnMessagePush();
            subscribeObject = client.Subscribe((Service.ServiceName, arg => OnMessagePush()));
            loopTask = new TaskCompletionSource<bool>();
            await loopTask.Task;
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        void INetTransfer.Close()
        {
            subscribeObject.Unsubscribe();
            while (isBusy > 0)//等处理线程退出
            {
                Thread.Sleep(10);
            }

            loopTask?.SetResult(true);
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        void INetTransfer.LoopComplete()
        {
            subscribeObject.Dispose();
            client.Dispose();
        }
        #region 消息处理

        /// <summary>
        /// 异常守护
        /// </summary>
        private async Task Guard()
        {
            using var client = new CSRedisClient(Option.ConnectionString);
            //处理错误重新入列
            while (true)
            {
                var key = client.LPop(errList);
                if (string.IsNullOrEmpty(key))
                {
                    break;
                }

                LogRecorder.Trace("异常消息重新入列:{0}", key);
                client.LPush(jobList, key);
            }
            //非正常处理还原
            while (ZeroFlowControl.IsAlive)
            {
                Thread.Sleep(Option.GuardCheckTime);

                var key = await client.LPopAsync(bakList);
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                var guard = $"guard:{Service.ServiceName}:{key}";
                if (await client.SetNxAsync(guard, "Guard"))
                {
                    client.Expire(guard, Option.MessageLockTime);
                    LogRecorder.Trace("超时消息重新入列:{0}", key);
                    await client.LPushAsync(jobList, key);
                    await client.DelAsync(guard);
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
                    var id = client.RPopLPush(jobList, bakList);
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    var key = $"msg:{Service.ServiceName}:{id}";
                    var guard = $"guard:{Service.ServiceName}:{id}";
                    if (await client.SetNxAsync(guard, "Guard"))
                    {
                        client.Expire(guard, Option.MessageLockTime);

                        var str = client.Get(key);
                        if (string.IsNullOrEmpty(str))
                        {
                            LogRecorder.Debug("Empty:{0}", key);
                            await client.DelAsync(key);
                            await client.LRemAsync(bakList, 0, id);
                            await client.DelAsync(guard);
                            continue;
                        }
                        else
                        {
                            MessageItem item;
                            try
                            {
                                item = JsonHelper.DeserializeObject<MessageItem>(str);
                            }
                            catch
                            {
                                LogRecorder.Debug("Error:{0} =>{1}", key, str);
                                await client.DelAsync(key);
                                await client.LRemAsync(bakList, 0, id);
                                await client.DelAsync(guard);
                                continue;
                            }
                            try
                            {
                                item.ID = id;
                                item.Topic = Service.ServiceName;
                                switch (await MessageProcess.OnMessagePush(Service, item))
                                {
                                    default:
                                        //case MessageState.Cancel:
                                        //case MessageState.Exception:
                                        await client.LPushAsync(errList, id);
                                        break;
                                    case MessageState.FormalError:
                                    case MessageState.Success:
                                        await client.DelAsync(key);
                                        break;
                                    case MessageState.Failed:
                                        if (Option.FailedIsError)
                                        {
                                            await client.LPushAsync(errList, id);
                                        }

                                        break;
                                    case MessageState.NoSupper:
                                        if (Option.NoSupperIsError)
                                        {
                                            await client.LPushAsync(errList, id);
                                        }

                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogRecorder.Exception(ex);
                                await client.LPushAsync(errList, id);
                            }
                        }
                        await client.LRemAsync(bakList, 0, id);
                        await client.DelAsync(guard);
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    Thread.Sleep(300);
                }
            }
        }
        #endregion
    }
}
