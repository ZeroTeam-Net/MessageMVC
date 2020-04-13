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
    ///     ����
    /// </summary>
    internal class RouteCache : IMessageMiddleware
    {

        #region IMessageMiddleware

        /// <summary>
        /// �㼶
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;


        /// <summary>
        /// ��Ϣ�м���Ĵ���Χ
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope =>
            GatewayOption.Instance.EnableCache
                 ? MessageHandleScope.Prepare | MessageHandleScope.End
                 : MessageHandleScope.None;

        /// <summary>
        /// ��ǰ������
        /// </summary>
        MessageProcessor IMessageMiddleware.Processor { get; set; }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <param name="service">��ǰ����</param>
        /// <param name="message">��ǰ��Ϣ</param>
        /// <param name="tag">��չ��Ϣ</param>
        /// <param name="next">��һ��������</param>
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
        /// ����
        /// </summary>
        /// <param name="message">��ǰ��Ϣ</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (message is HttpMessage httpMessage)
                CacheResult(httpMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region ����

        /// <summary>
        ///     ��ǰ���õĻ������ö���
        /// </summary>
        private ApiCacheOption CacheSetting;

        /// <summary>
        ///     �����
        /// </summary>
        private string CacheKey;

        /// <summary>
        ///     ��黺��
        /// </summary>
        /// <returns>�Ƿ���Ҫ��������</returns>
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
                LogRecorder.MonitorTrace(() => $"����δ����,ֱ�ӷ��ʺ�˷����� {CacheKey}");
                return true;
            }
            if (cacheData.Success && (cacheData.UpdateTime > DateTime.Now || cacheData.IsLoading > 0))
            {
                data.Result = cacheData.Content;
                LogRecorder.MonitorTrace(() => $"���л���,ֱ��ʹ�� {CacheKey}");
                return false;
            }
            //һ�����룬�����ĵȴ����óɹ�
            if (Interlocked.Increment(ref cacheData.IsLoading) == 1)
            {
                LogRecorder.MonitorTrace(() => $"������ͻ,ֱ�ӷ��ʺ�˷����� {CacheKey}");
                return false;
            }
            Interlocked.Decrement(ref cacheData.IsLoading);
            //�ȴ����óɹ�
            LogRecorder.MonitorTrace(() => $"�ȴ�ǰһ�����ʽ��� {CacheKey}");
            var task = new TaskCompletionSource<string>();
            cacheData.Waits.Add(task);
            data.Result = await task.Task;
            LogRecorder.MonitorTrace(() => $"ֱ��ʹ�÷��ؽ��� {CacheKey}");
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
        ///     ���淵��ֵ
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