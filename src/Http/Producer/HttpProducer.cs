using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class HttpProducer : IMessageProducer
    {
        #region IMessageProducer

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
        void IMessageProducer.Initialize()
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

        private async Task<(bool, string)> PostAsync(string service, string title, string content)
        {
            if (!ServiceMap.TryGetValue(service, out var name))
            {
                name = defName;
            }

            var client = httpClientFactory.CreateClient(name);
            var response = await client.PostAsync($"/{service}/{title}", new StringContent(content ?? ""));

            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }
            var json = await response.Content.ReadAsStringAsync();
            return (true, json);
        }

        private (bool, string) Post(string service, string title, string content)
        {
            if (!ServiceMap.TryGetValue(service, out var name))
            {
                name = defName;
            }

            var client = httpClientFactory.CreateClient(name);
            var response = client.PostAsync($"{name}/{service}/{title}", new StringContent(content ?? "")).Result;

            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }
            var json = response.Content.ReadAsStringAsync().Result;
            return (true, json);
        }
        /// <inheritdoc/>
        public string Producer(string service, string title, string content)
        {
            return Post(service, title, content).Item2;
        }

        TRes IMessageProducer.Producer<TArg, TRes>(string service, string title, TArg content)
        {
            var res = Post(service, title, JsonHelper.SerializeObject(content));
            return res.Item1 ? default : JsonHelper.DeserializeObject<TRes>(res.Item2);
        }

        /// <inheritdoc/>
        public void Producer<TArg>(string service, string title, TArg content)
        {
            Post(service, title, JsonHelper.SerializeObject(content));
        }
        TRes IMessageProducer.Producer<TRes>(string service, string title)
        {
            var res = Post(service, title, null);
            return res.Item1 ? default : JsonHelper.DeserializeObject<TRes>(res.Item2);
        }


        async Task<string> IMessageProducer.ProducerAsync(string service, string title, string content)
        {
            var res = await PostAsync(service, title, content);
            return res.Item2;
        }

        async Task<TRes> IMessageProducer.ProducerAsync<TArg, TRes>(string service, string title, TArg content)
        {
            var res = await PostAsync(service, title, JsonHelper.SerializeObject(content));
            return res.Item1 ? default : JsonHelper.DeserializeObject<TRes>(res.Item2);
        }
        Task IMessageProducer.ProducerAsync<TArg>(string service, string title, TArg content)
        {
            return PostAsync(service, title, string.Empty);
        }

        async Task<TRes> IMessageProducer.ProducerAsync<TRes>(string service, string title)
        {
            var res = await PostAsync(service, title, string.Empty);
            return res.Item1 ? default : JsonHelper.DeserializeObject<TRes>(res.Item2);
        }
        #endregion
    }
}