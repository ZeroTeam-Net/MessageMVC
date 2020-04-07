using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MicroZero.Http.Gateway
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
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Prepare | MessageHandleScope.End;

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
        async Task<bool> IMessageMiddleware.Prepare(IService service, IMessageItem message, object tag)
        {
            return await LoadCache(message as HttpMessage);
        }


        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IMessageItem message)
        {
            CacheResult(message as HttpMessage);
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
        /// <returns>取到缓存，可以直接返回</returns>
        private async Task<bool> LoadCache(HttpMessage data)
        {
            var api = $"{data.ApiHost}/{data.ApiName}";
            if (!CacheMap.TryGetValue(api, out CacheSetting))
            {
                return false;
            }
            var kb = new StringBuilder();
            kb.Append(api);
            kb.Append('?');
            if (CacheSetting.Feature.HasFlag(CacheFeature.Keys))
            {
                foreach (var key in CacheSetting.Keys)
                {
                    if (data.Arguments.TryGetValue(key, out var value))
                    {
                        kb.Append($"{key}={value}&");
                    }
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(data.HttpContent))
                    {
                        var dic = JsonHelper.DeserializeObject<Dictionary<string, string>>(data.HttpContent);

                        foreach (var key in CacheSetting.Keys)
                        {
                            if (data.Arguments.TryGetValue(key, out var value))
                            {
                                kb.Append($"{key}={value}&");
                            }
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
                    foreach (var kv in data.Arguments)
                    {
                        kb.Append($"{kv.Key}={kv.Value}&");
                    }
                    if (!string.IsNullOrWhiteSpace(data.HttpContent))
                    {
                        kb.Append(data.HttpContent);
                    }
                }
            }

            CacheKey = kb.ToString();
            if (!Cache.TryGetValue(CacheKey, out var cacheData))
            {
                Cache.TryAdd(CacheKey, new CacheData
                {
                    IsLoading = 1,
                    Content = ApiResultHelper.NoReadyJson
                });
                LogRecorder.MonitorTrace(() => $"Cache Load {CacheKey}");
                return false;
            }
            if (cacheData.Success && (cacheData.UpdateTime > DateTime.Now || cacheData.IsLoading > 0))
            {
                data.Result = cacheData.Content;
                LogRecorder.MonitorTrace(() => $"Cache by {CacheKey}");
                return true;
            }
            //一个载入，其它的等待调用成功
            if (Interlocked.Increment(ref cacheData.IsLoading) == 1)
            {
                LogRecorder.MonitorTrace(() => $"Cache update {CacheKey}");
                return false;
            }
            Interlocked.Decrement(ref cacheData.IsLoading);
            //等待调用成功
            LogRecorder.MonitorTrace(() => $"Cache wait {CacheKey}");
            var task = new TaskCompletionSource<string>();
            cacheData.Waits.Add(task);
            data.Result = await task.Task;
            return true;
        }

        /// <summary>
        ///     缓存返回值
        /// </summary>
        /// <param name="data"></param>
        private void CacheResult(HttpMessage data)
        {
            if (CacheSetting != null && CacheKey != null)
            {
                if (Cache.TryGetValue(CacheKey, out var cd))
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

                Cache[CacheKey] = new CacheData
                {
                    Content = data.Result,
                    Success = data.IsSucceed,
                    IsLoading = 0,
                    UpdateTime = DateTime.Now.AddSeconds(CacheSetting.FlushSecond)
                };
                LogRecorder.MonitorTrace($"Cache succeed {CacheKey}");
            }
            if (/*data.IsSucceed &&*/ UpdateMap.TryGetValue($"{data.ApiHost}/{data.ApiName}", out var uc))
            {
                var kb = new StringBuilder();
                kb.Append(uc.CacheApi);
                kb.Append('?');

                foreach (var map in uc.Map)
                {
                    if (data.Arguments.TryGetValue(map.Key, out var value))
                    {
                        kb.Append($"{map.Value}={value}&");
                    }
                }

                //var jObject = (JObject)JsonConvert.DeserializeObject(data.Result);

                //foreach (var map in uc.Map)
                //{
                //    if (jObject.TryGetValue(map.Key, out var value))
                //        kb.Append($"{map.Value}={value}&");
                //}
                var key = kb.ToString();
                if (Cache.TryRemove(key, out var cd))
                {
                    LogRecorder.MonitorTrace($"Cache remove {key}");
                    foreach (var task in cd.Waits.ToArray())
                    {
                        task.TrySetResult(cd.Content);
                    }
                }
            }
        }

        #endregion

        #region 配置

        static RouteCache()
        {
            ConfigurationManager.RegistOnChange(LoadOption, true);
        }

        /// <summary>
        ///     缓存配置
        /// </summary>
        static CacheOption Option = new CacheOption();


        /// <summary>
        ///     缓存数据
        /// </summary>
        static readonly ConcurrentDictionary<string, CacheData> Cache = new ConcurrentDictionary<string, CacheData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     路由配置
        /// </summary>
        static Dictionary<string, ApiCacheOption> CacheMap { get; set; }

        /// <summary>
        ///     路由配置
        /// </summary>
        static Dictionary<string, CacheFlushOption> UpdateMap { get; set; }

        static void LoadOption()
        {
            Cache.Clear();
            CacheMap = new Dictionary<string, ApiCacheOption>(StringComparer.OrdinalIgnoreCase);
            UpdateMap = new Dictionary<string, CacheFlushOption>(StringComparer.OrdinalIgnoreCase);
            try
            {
                Option = ConfigurationManager.Option<CacheOption>("Gateway:Chache") ?? new CacheOption();

                if (Option?.Api != null)
                {
                    foreach (var setting in Option.Api)
                    {
                        setting.Initialize();
                        if (!CacheMap.ContainsKey(setting.Api))
                        {
                            CacheMap.Add(setting.Api, setting);
                        }
                        else
                        {
                            CacheMap[setting.Api] = setting;
                        }
                    }
                }

                if (Option?.Trigger != null)
                {
                    foreach (var setting in Option.Trigger)
                    {
                        if (!UpdateMap.ContainsKey(setting.TriggerApi))
                        {
                            UpdateMap.Add(setting.TriggerApi, setting);
                        }
                        else
                        {
                            UpdateMap[setting.TriggerApi] = setting;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
            }
        }

        #endregion

    }
}