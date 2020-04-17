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
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class HttpPoster : MessagePostBase, IMessagePoster
    {
        #region HttpClientFactory

        /// <summary>
        /// 服务到HttpClientName的查找表.
        /// </summary>
        private readonly Dictionary<string, string> ServiceMap = new Dictionary<string, string>();
        private IHttpClientFactory httpClientFactory;
        private string defName;


        private readonly Dictionary<string, HttpClientOption> Options = new Dictionary<string, HttpClientOption>();

        void LoadOption()
        {
            var dirStr = ConfigurationManager.Get<HttpClientOption[]>("MessageMVC:HttpClient");
            if (dirStr != null)
            {
                foreach (var kv in dirStr)
                {
                    if (Options.ContainsKey(kv.Name))
                    {
                        Options[kv.Name] = kv;
                        continue;
                    }
                    Options.TryAdd(kv.Name, kv);
                    DependencyHelper.ServiceCollection.AddHttpClient(kv.Name);

                    if (string.IsNullOrEmpty(kv.Services))
                    {
                        defName = kv.Name;
                        continue;
                    }
                    foreach (var service in kv.Services.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (ServiceMap.ContainsKey(service))
                            ServiceMap[service] = kv.Name;
                        else
                            ServiceMap.Add(service, kv.Name);
                    }
                }
                DependencyHelper.Update();
            }
            httpClientFactory = DependencyHelper.Create<IHttpClientFactory>();

        }
        #endregion

        #region IMessagePoster

        /// <summary>
        /// 名称
        /// </summary>
        string IZeroDependency.Name => nameof(HttpPoster);

        StationStateType state;

        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType IMessagePoster.State { get => state; set => state = value; }

        /// <summary>
        ///     初始化
        /// </summary>
        void IMessagePoster.Initialize()
        {
            Initialize();
            state = StationStateType.Run;
            ConfigurationManager.RegistOnChange("MessageMVC:HttpClient", LoadOption, true);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            if (!ServiceMap.TryGetValue(message.Topic, out var name))
            {
                name = defName;
            }
            var result = message.ToMessageResult();
            LogRecorder.BeginStepMonitor("[HttpPoster.Post]");
            try
            {
                var client = httpClientFactory.CreateClient(name);
                client.BaseAddress = new Uri(Options[name].Url);
                client.DefaultRequestHeaders.Add("User-Agent", MessageRouteOption.AgentName);
                LogRecorder.MonitorTrace(() => $"URL : {client.BaseAddress }{message.Topic}/{message.Title}");
                using var response = await client.PostAsync(
                    $"/{message.Topic}/{message.Title}",
                    new StringContent(message.ToJson()));

                var json = await response.Content.ReadAsStringAsync();

                LogRecorder.MonitorTrace("StatusCode : {0}", response.StatusCode);
                if (!response.IsSuccessStatusCode)
                {
                    //BUG:太粗
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.NonAuthoritativeInformation:
                        case HttpStatusCode.ProxyAuthenticationRequired:
                        case HttpStatusCode.NetworkAuthenticationRequired:
                            result.State = MessageState.NonSupport;
                            result.RuntimeStatus = ApiResultHelper.Helper.NonSupport;
                            break;
                        default:
                            result.State = MessageState.NetworkError;
                            result.RuntimeStatus = ApiResultHelper.Helper.NetworkError;
                            break;
                    }
                }
                else if (JsonHelper.TryDeserializeObject<MessageResult>(json, out var re2))
                {
                    re2.DataState = MessageDataState.ResultOffline;
                    result = re2;
                }
                else
                {
                    result.State = MessageState.NetworkError;
                    result.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.NetworkError); ;
                }
                LogRecorder.MonitorTrace(result.Result);
                return result;
            }
            catch (Exception ex)
            {
                LogRecorder.MonitorTrace(() => $"发生异常.{ex.Message}");
                throw new MessageReceiveException("[HttpPoster] ", ex);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
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
            using (MonitorStepScope.CreateScope("[HttpPoster] {0}", url))
            {
                try
                {
                    var client = httpClientFactory.CreateClient(name);
                    var response = client.PostAsync(url, new StringContent(content ?? "")).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        LogRecorder.MonitorTrace("Error:{0}", response.StatusCode);
                        return (MessageState.NetworkError, null);
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
