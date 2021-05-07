using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Agebull.EntityModel.Common
{
    /// <summary>
    /// 内存数据
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public abstract class MemoryCacheData<TData> where TData : class, new()
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// 过期时长
        /// </summary>
        protected int ExpirationSecond { get; set; } = 60;

        /// <summary>
        /// 过期时长
        /// </summary>
        protected int TimeoutSecond { get; set; } = 5;

        /// <summary>
        /// 重试等待时长
        /// </summary>
        protected int RetryWaitSecond { get; set; } = 1;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        protected int MaxTryCount { get; set; } = 5;

        /// <summary>
        /// 内存数据
        /// </summary>
        protected TData Data { get; set; }

        //防止多次执行的守卫变量
        int IsLoading = 0;

        /// <summary>
        /// 载入过程的等待任务
        /// </summary>
        private readonly List<(DateTime, TaskCompletionSource<TData>)> waitTasks = new();

        /// <summary>
        /// 过期时间
        /// </summary>
        DateTime expiration;

        /// <summary>
        /// 取消令牌
        /// </summary>
        protected CancellationToken Token { get; private set; }

        /// <summary>
        /// 缓存名称
        /// </summary>
        public string CacheName { get;set; } 

        /// <summary>
        /// 构造
        /// </summary>
        protected MemoryCacheData()
        {
            CacheName = GetType().Name;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public Task Reset()
        {
            expiration = DateTime.MinValue;
            return LoadData();
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public Task Load(CancellationToken token)
        {
            Token = token;
            expiration = DateTime.MinValue;
            return LoadData();
        }

        /// <summary>
        /// 载入数据
        /// </summary>
        /// <returns></returns>
        public Task<TData> LoadData()
        {
            if (Data != null)
            {
                if (expiration < DateTime.Now && Interlocked.Increment(ref IsLoading) == 1)
                    ScopeRuner.RunScope($"{CacheName}-LoadData", LoadInner, ContextInheritType.None);
                return Task.FromResult(Data);
            }
            if (IsLoading < 0)
                throw new Exception($"{CacheName}.LoadData暂时不可用，请3秒后再试");

            var task = new TaskCompletionSource<TData>();
            waitTasks.Add((DateTime.Now, task));
            if (Interlocked.Increment(ref IsLoading) == 1)
            {
                ScopeRuner.RunScope($"{CacheName}-LoadData", LoadInner, ContextInheritType.None);
            }
            return task.Task;
        }

        /// <summary>
        /// 异常重试次数
        /// </summary>
        int tryCount;

        /// <summary>
        /// 读取策略
        /// </summary>
        /// <returns></returns>
        async Task LoadInner()
        {
            await Task.Yield();
            Logger.LogDebug("开始载入数据");
            using var di = Logger.BeginScope($"{CacheName}.LoadInner");
            try
            {
                Data = await DoLoad();
                expiration = DateTime.Now.AddSeconds(ExpirationSecond);
                if (Data != null)
                {
                    IsLoading = 0;
                    await Task.Delay(1);
                    foreach (var task in waitTasks.ToArray())
                    {
                        task.Item2.SetResult(Data);
                    }
                    waitTasks.Clear();
                    Logger.LogDebug("载入完成");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, CacheName);
                if (Data != null)
                {
                    expiration = DateTime.Now.AddSeconds(RetryWaitSecond);
                    return;
                }
            }
            var array = waitTasks.ToArray();
            if (++tryCount < MaxTryCount)
            {
                Logger.LogError($"{CacheName}.LoadInner未读到数据，延迟{RetryWaitSecond}秒后重试");

                foreach (var task in array)
                {
                    if ((DateTime.Now - task.Item1).TotalSeconds >= TimeoutSecond)
                    {
                        try
                        {
                            task.Item2.SetException(new TimeoutException($"{CacheName}.Load已超时({TimeoutSecond}秒)，宣告失败"));
                        }
                        catch
                        {
                        }
                    }
                    waitTasks.Remove(task);
                }

                await Task.Delay(RetryWaitSecond * 1000);
                ScopeRuner.RunScope($"{GetType().Name}-LoadData", LoadInner, ContextInheritType.None);
            }
            else
            {
                var msg = $"{CacheName}.Load重试{MaxTryCount}次未读到数据，宣告失败";
                Logger.LogError(msg);
                IsLoading = -1;//标记异常
                await Task.Delay(1);

                foreach (var task in array)
                {
                    try
                    {
                        task.Item2.TrySetException(new TimeoutException(msg));
                    }
                    catch
                    {
                    }
                }
                waitTasks.Clear();
                tryCount = 0;
                await Task.Delay(3000);
                IsLoading = 0;
            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        protected abstract Task<TData> DoLoad();
    }
}
