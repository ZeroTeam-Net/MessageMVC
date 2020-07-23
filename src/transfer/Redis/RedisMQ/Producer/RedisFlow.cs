using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using CSRedis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    ///     Redis流程
    /// </summary>
    internal class RedisFlow : IFlowMiddleware, IHealthCheck
    {
        #region IHealthCheck

        async Task<HealthInfo> IHealthCheck.Check()
        {
            var info = new HealthInfo
            {
                ItemName = nameof(RedisFlow),
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

        #region IFlowMiddleware

        /// <summary>
        /// 单例
        /// </summary>
        public static RedisFlow Instance = new RedisFlow();

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(RedisFlow);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        internal CSRedisClient client;

        /// <summary>
        /// 日志器
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(GetType().GetTypeName());
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Open()
        {
            Logger.Information("RedisBackPoster >>> Start");
            client = new CSRedisClient(RedisOption.Instance.ConnectionString);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Close()
        {
            client.Dispose();
            return Task.CompletedTask;
        }

        #endregion

    }
}
