using Agebull.Common.Logging;
using CSRedis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;
namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis生产者
    /// </summary>
    internal class CsRedisPoster : BackgroundPoster<QueueItem>, IHealthCheck, ILifeFlow, IZeroDiscover
    {
        #region Poster

        /// <summary>
        /// 征集周期管理器
        /// </summary>
        protected override ILifeFlow LifeFlow => this;

        /// <summary>
        /// 单例
        /// </summary>
        public static CsRedisPoster Instance = new CsRedisPoster();

        public CsRedisPoster()
        {
            Name = nameof(CsRedisPoster);
        }

        protected override TaskCompletionSource<IMessageResult> DoPost(QueueItem item)
        {
            Logger.Trace(() => $"[异步消息投递] {item.ID} 正在投递消息.{RedisOption.Instance.ConnectionString}");
            var res = new TaskCompletionSource<IMessageResult>();
            client
                .PublishAsync(item.Message.Service, SmartSerializer.SerializeMessage(item.Message))
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Logger.Trace(() => $"[异步消息投递] {item.ID} 发生异常({task.Exception.Message})");
                        res.TrySetResult(new MessageResult
                        {
                            ID = item.Message.ID,
                            Trace = item.Message.TraceInfo,
                            State = MessageState.NetworkError
                        });
                    }
                    else if (task.IsFaulted)
                    {
                        Logger.Trace(() => $"[异步消息投递] {item.ID} 操作取消");

                        res.TrySetResult(new MessageResult
                        {
                            ID = item.Message.ID,
                            Trace = item.Message.TraceInfo,
                            State = MessageState.Cancel
                        });
                    }
                    else
                    {
                        Logger.Trace(() => $"[异步消息投递] {item.ID} 投递成功");

                        res.TrySetResult(new MessageResult
                        {
                            ID = item.Message.ID,
                            Trace = item.Message.TraceInfo,
                            State = MessageState.Success
                        });
                    }
                });
            return res;
        }
        #endregion

        #region IHealthCheck

        async Task<HealthInfo> IHealthCheck.Check()
        {
            var info = new HealthInfo
            {
                ItemName = nameof(CsRedisPoster),
                Items = new List<HealthItem>()
            };

            info.Items.Add(await SetTest());
            info.Items.Add(await GetTest());
            info.Items.Add(await DelTest());
            return info;
        }

        private async Task<HealthItem> SetTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Set"
            };
            try
            {
                DateTime start = DateTime.Now;
                if (!await client.SetAsync("_health_", "c", 10))
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    item.Level = 0;
                }
                else
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (item.Value < 10)
                        item.Level = 5;
                    else if (item.Value < 100)
                        item.Level = 4;
                    else if (item.Value < 500)
                        item.Level = 3;
                    else if (item.Value < 3000)
                        item.Level = 2;
                    else item.Level = 1;
                }
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }

        private async Task<HealthItem> GetTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Get"
            };
            try
            {
                DateTime start = DateTime.Now;
                if (await client.GetAsync("_health_") != "c")
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    item.Level = 0;
                }
                else
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (item.Value < 10)
                        item.Level = 5;
                    else if (item.Value < 100)
                        item.Level = 4;
                    else if (item.Value < 500)
                        item.Level = 3;
                    else if (item.Value < 3000)
                        item.Level = 2;
                    else item.Level = 1;
                }
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }
        private async Task<HealthItem> DelTest()
        {
            HealthItem item = new HealthItem
            {
                ItemName = "Del"
            };
            try
            {
                DateTime start = DateTime.Now;
                if (await client.DelAsync("_health_") != 1)
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    item.Level = 0;
                }
                else
                {
                    item.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (item.Value < 10)
                        item.Level = 5;
                    else if (item.Value < 100)
                        item.Level = 4;
                    else if (item.Value < 500)
                        item.Level = 3;
                    else if (item.Value < 3000)
                        item.Level = 2;
                    else item.Level = 1;
                }
                await client.DelAsync("_health_");
            }
            catch (Exception ex)
            {
                item.Level = -1;
                item.Details = ex.Message;
            }
            return item;
        }

        #endregion

        #region ILifeFlow

        internal CSRedisClient client;

        /// <summary>
        /// 发现期间开启任务
        /// </summary>
        Task IZeroDiscover.Discovery()
        {
            try
            {
                AsyncPost = RedisOption.Instance.AsyncPost;
                client = new CSRedisClient(RedisOption.Instance.ConnectionString);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            DoStart();
            RecordLog(LogLevel.Information, $"{Name}已开启");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
       async Task ILifeFlow.Destroy()
        {
            await Destroy();
            client?.Dispose();
            RecordLog(LogLevel.Information, $"{Name}已关闭");
        }

        #endregion
    }
}
