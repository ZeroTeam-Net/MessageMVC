using Agebull.Common.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Wechart;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     缓存
    /// </summary>
    internal class RouteCache : IMessageMiddleware
    {

        #region IMessageMiddleware

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;


        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope =>
            GatewayOption.Instance.EnableCache
                 ? MessageHandleScope.Prepare | MessageHandleScope.End
                 : MessageHandleScope.None;

        /// <summary>
        /// 当前处理器
        /// </summary>
        MessageProcessor IMessageMiddleware.Processor { get; set; }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (message.Topic == WxPayOption.Instance.CallbackPath)
                return true;
            if (message is HttpMessage httpMessage)
                return await LoadCache(httpMessage);
            return true;
        }


        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (message is HttpMessage httpMessage)
                CacheResult(httpMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region 数据

        /// <summary>
        ///     当前适用的缓存设置对象
        /// </summary>
        private ApiCacheOption CacheSetting;

        /// <summary>
        ///     缓存键
        /// </summary>
        private string CacheKey;

        /// <summary>
        ///     检查缓存
        /// </summary>
        /// <returns>是否需要后续动作</returns>
        private async Task<bool> LoadCache(HttpMessage data)
        {
            var api = $"{data.ApiHost}/{data.ApiName}";
            if (!CacheOption.CacheMap.TryGetValue(api, out CacheSetting))
            {
                return true;
            }
            BuildCacheKey(data, api);
            if (!CacheOption.Cache.TryGetValue(CacheKey, out var cacheData))
            {
                CacheOption.Cache.TryAdd(CacheKey, new CacheData
                {
                    IsLoading = 1,
                    Content = ApiResultHelper.NoReadyJson
                });
                LogRecorder.MonitorTrace(() => $"缓存未命中,直接访问后端服务器 {CacheKey}");
                return true;
            }
            if (cacheData.Success && (cacheData.UpdateTime > DateTime.Now || cacheData.IsLoading > 0))
            {
                data.Result = cacheData.Content;
                LogRecorder.MonitorTrace(() => $"命中缓存,直接使用 {CacheKey}");
                return false;
            }
            //一个载入，其它的等待调用成功
            if (Interlocked.Increment(ref cacheData.IsLoading) == 1)
            {
                LogRecorder.MonitorTrace(() => $"并发冲突,直接访问后端服务器 {CacheKey}");
                return false;
            }
            Interlocked.Decrement(ref cacheData.IsLoading);
            //等待调用成功
            LogRecorder.MonitorTrace(() => $"等待前一个访问结束 {CacheKey}");
            var task = new TaskCompletionSource<string>();
            cacheData.Waits.Add(task);
            data.Result = await task.Task;
            LogRecorder.MonitorTrace(() => $"直接使用返回结束 {CacheKey}");
            return true;
        }

        private void BuildCacheKey(HttpMessage data, string api)
        {
            var kb = new StringBuilder();
            kb.Append(api);
            kb.Append('?');
            if (CacheSetting.Feature.HasFlag(CacheFeature.Keys))
            {
                try
                {

                    foreach (var key in CacheSetting.Argument)
                    {
                        var value = data.GetValueArgument(key, 0);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            kb.Append($"{key}={value}&");
                        }
                    }
                }
                catch// (Exception)
                {
                }
            }
            else
            {
                if (CacheSetting.Feature.HasFlag(CacheFeature.Bear))
                {
                    kb.Append($"token={data.Trace.Token}&");
                }
                if (CacheSetting.Feature.HasFlag(CacheFeature.QueryString))
                {
                    foreach (var kv in data.HttpArguments)
                    {
                        kb.Append($"{kv.Key}={kv.Value}&");
                    }
                    if (!string.IsNullOrWhiteSpace(data.HttpContent))
                    {
                        kb.Append(data.HttpContent);
                    }
                }
                if (CacheSetting.Feature.HasFlag(CacheFeature.Form))
                {
                    foreach (var kv in data.HttpForms)
                    {
                        kb.Append($"{kv.Key}={kv.Value}&");
                    }
                }
                if (CacheSetting.Feature.HasFlag(CacheFeature.Content))
                {
                    if (!string.IsNullOrWhiteSpace(data.HttpContent))
                    {
                        kb.Append(data.HttpContent);
                    }
                }
            }

            CacheKey = kb.ToString();
        }

        /// <summary>
        ///     缓存返回值
        /// </summary>
        /// <param name="data"></param>
        private void CacheResult(HttpMessage data)
        {
            if (CacheSetting == null || CacheKey == null)
            {
                return;
            }
            if (CacheOption.Cache.TryGetValue(CacheKey, out var cd))
            {
                var res = data.IsSucceed ? data.Result : cd.Content;
                foreach (var task in cd.Waits.ToArray())
                {
                    task.TrySetResult(res);
                }
            }
            if (!data.IsSucceed && !CacheSetting.Feature.HasFlag(CacheFeature.NetError))
            {
                return;
            }

            CacheOption.Cache[CacheKey] = new CacheData
            {
                Content = data.Result,
                Success = data.IsSucceed,
                IsLoading = 0,
                UpdateTime = DateTime.Now.AddSeconds(CacheSetting.FlushSecond)
            };
            LogRecorder.MonitorTrace($"Cache succeed {CacheKey}");
        }

        #endregion


    }
}