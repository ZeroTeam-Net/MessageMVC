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

                    if (kv.Name == "Default")
                    {
                        defName = kv.Name;
                        continue;
                    }
                    if (kv.Services != null)
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
            IMessageResult result;
            LogRecorder.BeginStepMonitor("[HttpPoster.Post]");
            try
            {
                message.Offline();
                var client = httpClientFactory.CreateClient(name);
                client.BaseAddress = new Uri(Options[name].Url);
                client.DefaultRequestHeaders.Add("User-Agent", MessageRouteOption.AgentName);
                LogRecorder.MonitorDetails(() => $"URL : {client.BaseAddress }{message.Topic}/{message.Title}");
                using var response = await client.PostAsync(
                    $"/{message.Topic}/{message.Title}",
                    new StringContent(SmartSerializer.SerializeMessage(message)));

                var json = await response.Content.ReadAsStringAsync();

                LogRecorder.MonitorDetails(() => $"StatusCode : {response.StatusCode}");

                if (SmartSerializer.TryDeserialize<MessageResult>(json, out var re2))
                {
                    result = re2;
                    re2.DataState = MessageDataState.ResultOffline;
                }
                else
                {
                    result = new MessageResult
                    {
                        ID = message.ID,
                        Trace = message.Trace
                    };
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            result.State = MessageState.Success;
                            break;
                        case HttpStatusCode.MethodNotAllowed:
                            result.State = MessageState.Unhandled;
                            break;
                        case HttpStatusCode.NotFound:
                            result.State = MessageState.Unhandled;
                            break;
                        case HttpStatusCode.BadRequest:
                            result.State = MessageState.FormalError;
                            break;
                        case HttpStatusCode.RequestTimeout:
                            result.State = MessageState.Cancel;
                            break;
                        case HttpStatusCode.InternalServerError:
                            result.State = MessageState.BusinessError;
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            result.State = MessageState.NetworkError;
                            break;
                        case HttpStatusCode.ProxyAuthenticationRequired:
                        case HttpStatusCode.NetworkAuthenticationRequired:
                        case HttpStatusCode.NonAuthoritativeInformation:
                            result.State = MessageState.Deny;
                            break;
                        case HttpStatusCode.Accepted:
                            result.State = MessageState.AsyncQueue;
                            break;
                        case HttpStatusCode.ExpectationFailed:
                            result.State = MessageState.FrameworkError;
                            break;
                        default:
                            result.State = MessageState.NetworkError;
                            break;
                    }
                }
                LogRecorder.MonitorDetails(() => $"State : {result.State} Result : {result.Result}");
                return result;
            }
            catch (Exception ex)
            {
                LogRecorder.MonitorInfomation(() => $"发生异常.{ex.Message}");
                message.State = MessageState.NetworkError;
                return null;//直接使用状态
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
        }

        #endregion
    }
}
