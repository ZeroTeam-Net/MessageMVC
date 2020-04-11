using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class HttpPoster : NewtonJsonSerializeProxy, IMessagePoster
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



        /// <summary>
        ///     初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            var dirStr = ConfigurationManager.Get<HttpClientOption[]>("MessageMVC:HttpClient");
            foreach (var kv in dirStr)
            {
                IocHelper.ServiceCollection.AddHttpClient(kv.Name, config =>
                {
                    config.BaseAddress = new Uri(kv.Url);
                    config.DefaultRequestHeaders.Add("User-Agent", MessageRouteOption.AgentName);
                });
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
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IInlineMessage> IMessagePoster.Post(IMessageItem message)
        {
            if (!ServiceMap.TryGetValue(message.Topic, out var name))
            {
                name = defName;
            }
            if (message is IInlineMessage inline)
            {
                inline.Offline(this);
            }
            else
            {
                inline = message.ToInline();
            }
            using (MonitorStepScope.CreateScope("[HttpPoster] {0}/{1}/{2}", defName, inline.Topic, inline.Title))
            {
                try
                {
                    var client = httpClientFactory.CreateClient(name);
                    using var response = await client.PostAsync($"/{inline.Topic}/{inline.Title}", new StringContent(inline.ToJson()));

                    inline.Result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        inline.State = MessageState.Success;
                    }
                    else
                    {
                        //BUG:太粗
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                            case HttpStatusCode.NetworkAuthenticationRequired:
                            case HttpStatusCode.NonAuthoritativeInformation:
                            case HttpStatusCode.ProxyAuthenticationRequired:
                                inline.State = MessageState.NoSupper;
                                break;
                            default:
                                inline.State = MessageState.NetError;
                                break;
                        }
                        LogRecorder.MonitorTrace("Error:{0}", response.StatusCode);
                        return inline;
                    }
                    LogRecorder.MonitorTrace(inline.Result);
                }
                catch (HttpRequestException ex)
                {
                    LogRecorder.MonitorTrace("Error : {0}", ex.Message);
                    inline.State = MessageState.NetError;
                    throw new MessageReceiveException("[HttpPoster] ", ex);
                }
            }
            return inline;
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
            using (MonitorStepScope.CreateScope("[HttpPoster] {0}", url))
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
