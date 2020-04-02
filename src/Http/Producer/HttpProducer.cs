using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class HttpProducer : IMessagePoster
    {
        #region IMessagePoster

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        /// 服务到HttpClientName的查找表.
        /// </summary>
        private readonly Dictionary<string, string> ServiceMap = new Dictionary<string, string>();
        private IHttpClientFactory httpClientFactory;
        private string defName;

        private class HttpClientOption
        {
            public string Name { get; set; }

            public string Url { get; set; }
            public string Services { get; set; }
        }

        /// <summary>
        ///     初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            var dirStr = ConfigurationManager.Get<HttpClientOption[]>("HttpClient");
            foreach (var kv in dirStr)
            {
                IocHelper.ServiceCollection.AddHttpClient(kv.Name, config => config.BaseAddress = new Uri(kv.Url));
                if (string.IsNullOrEmpty(kv.Services))
                {
                    defName = kv.Name;
                }
                else
                {
                    foreach (var service in kv.Services.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        ServiceMap.Add(service, kv.Name);
                    }
                }
            }
            IocHelper.Update();
            httpClientFactory = IocHelper.Create<IHttpClientFactory>();

        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns></returns>
        async Task<(MessageState state, string result)> IMessagePoster.Post(IMessageItem item)
        {
            if (!ServiceMap.TryGetValue(item.Topic, out var name))
            {
                name = defName;
            }

            using (MonitorStepScope.CreateScope("[HttpProducer] {0}/{1}/{2}", defName, item.Topic, item.Title))
            {
                try
                {
                    var client = httpClientFactory.CreateClient(name);
                    var response = await client.PostAsync($"/{item.Topic}/{item.Title}", new StringContent(item.Content ?? ""));

                    if (!response.IsSuccessStatusCode)
                    {
                        LogRecorder.MonitorTrace("Error:{0}", response.StatusCode);
                        return (MessageState.NetError, null);
                    }
                    var result = await response.Content.ReadAsStringAsync();
                    LogRecorder.MonitorTrace(result);
                    return (MessageState.Success, result);
                }
                catch (HttpRequestException ex)
                {
                    LogRecorder.MonitorTrace("Error : {0}", ex.Message);
                    throw new NetTransferException(ex.Message, ex);
                }
            }
        }

        #endregion
    }
}
/*
 
        private (MessageState success, string result) Post(string service, string title, string content)
        {
            if (!ServiceMap.TryGetValue(service, out var name))
            {
                name = defName;
            }
            var url = $"{name}/{service}/{title}";
            using (MonitorStepScope.CreateScope("[HttpProducer] {0}", url))
            {
                try
                {
                    var client = httpClientFactory.CreateClient(name);
                    var response = client.PostAsync(url, new StringContent(content ?? "")).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        LogRecorder.MonitorTrace("Error:{0}", response.StatusCode);
                        return (MessageState.NetError, null);
                    }
                    var result = response.Content.ReadAsStringAsync().Result;
                    LogRecorder.MonitorTrace(result);
                    return (MessageState.Success, result);
                }
                catch (HttpRequestException ex)
                {
                    LogRecorder.MonitorTrace("Error : {0}", ex.Message);
                    throw new NetTransferException(ex.Message,ex);
                }
            }
        }
        /// <inheritdoc/>
        public string Producer(string service, string title, string content)
        {
            return Post(service, title, content).result;
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string service, string title, TArg content)
        {
            var (success, result) = Post(service, title, JsonHelper.SerializeObject(content));
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }

        /// <inheritdoc/>
        public void Producer<TArg>(string service, string title, TArg content)
        {
            Post(service, title, JsonHelper.SerializeObject(content));
        }
        TRes IMessagePoster.Producer<TRes>(string service, string title)
        {
            var (success, result) = Post(service, title, null);
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }


        async Task<string> IMessagePoster.ProducerAsync(string service, string title, string content)
        {
            var (_, result) = await PostAsync(service, title, content);
            return result;
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string service, string title, TArg content)
        {
            var (success, result) = await PostAsync(service, title, JsonHelper.SerializeObject(content));
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }
        Task IMessagePoster.ProducerAsync<TArg>(string service, string title, TArg content)
        {
            return PostAsync(service, title, string.Empty);
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TRes>(string service, string title)
        {
            var (success, result) = await PostAsync(service, title, string.Empty);
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }
*/
