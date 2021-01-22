using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     Http生产者
    /// </summary>
    public class HttpPoster : MessagePostBase, IMessagePoster
    {
        #region IMessagePoster

        /// <summary>
        /// 名称
        /// </summary>
        string IZeroDependency.Name => nameof(HttpPoster);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            if (!HttpClientOption.ServiceMap.TryGetValue(message.Service, out var name))
            {
                name = HttpClientOption.DefaultName;
            }
            FlowTracer.BeginDebugStepMonitor("[HttpPoster.Post]");
            try
            {
                message.ArgumentOffline();
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                if (client.BaseAddress == null)
                {
                    FlowTracer.MonitorError(() => $"[{message.Service}/{message.Method}]服务未注册");
                    message.State = MessageState.Unhandled;
                    return null;//直接使用状态
                }
                var uri = new Uri($"{client.BaseAddress }{message.Service}/{message.Method}");
                FlowTracer.MonitorDetails(() =>
                {
                    return $"URL : {uri.OriginalString}";
                });
                using var content = new StringContent(SmartSerializer.SerializeRequest(message));
                using var requestMessage = new HttpRequestMessage
                {
                    RequestUri = uri,
                    Content = content,
                    Method = HttpMethod.Post
                };

                requestMessage.Headers.Add("x-zmvc-ver", message.ID);

                using var response = await client.SendAsync(requestMessage);
                if (response.Headers.TryGetValues("x-zmvc-state", out var state))
                {
                    message.State = Enum.Parse<MessageState>(state.First());
                }
                else
                {
                    message.State = MessageState.NetworkError;
                }
                message.Result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.BadGateway)
                {
                    message.State = HttpCodeToMessageState(response.StatusCode);
                    message.Result = null;
                }
                if (string.IsNullOrEmpty(message.Result))
                    message.OfflineResult();
                message.DataState |= MessageDataState.ResultOffline;
                FlowTracer.MonitorDetails(() => $"StatusCode : {response.StatusCode} | State : {message.State} | Result : {message.Result}");
            }
            catch (HttpRequestException ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                message.State = MessageState.Unsend;
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                message.State = MessageState.NetworkError;
            }
            finally
            {
                FlowTracer.EndDebugStepMonitor();
            }
            return null;//直接使用状态
        }

        static MessageState HttpCodeToMessageState(HttpStatusCode httpStatusCode)
        {
            switch (httpStatusCode)
            {
                case HttpStatusCode.OK:
                    return MessageState.Success;

                case HttpStatusCode.NotFound:
                case HttpStatusCode.MethodNotAllowed:
                    return MessageState.Unhandled;

                case HttpStatusCode.BadRequest:
                    return MessageState.FormalError;

                case HttpStatusCode.RequestTimeout:
                    return MessageState.Cancel;

                case HttpStatusCode.InternalServerError:
                    return MessageState.BusinessError;

                case HttpStatusCode.ServiceUnavailable:
                    return MessageState.NetworkError;

                case HttpStatusCode.ProxyAuthenticationRequired:
                case HttpStatusCode.NetworkAuthenticationRequired:
                case HttpStatusCode.NonAuthoritativeInformation:
                    return MessageState.Deny;

                case HttpStatusCode.Accepted:
                    return MessageState.AsyncQueue;

                case HttpStatusCode.ExpectationFailed:
                    return MessageState.FrameworkError;

                default:
                    return MessageState.NetworkError;

            }
        }
        #endregion

        #region 访问外部

        /// <summary>
        /// 访问外部
        /// </summary>
        /// <param name="service">服务名称，用于查找HttpClient，不会与api拼接</param>
        /// <param name="api">完整的接口名称+参数</param>
        /// <returns></returns>
        public static async Task<(MessageState state, string res)> OutGet(string service, string api)
        {
            if (!HttpClientOption.ServiceMap.TryGetValue(service, out var name))
            {
                name = HttpClientOption.DefaultName;
            }
            FlowTracer.BeginStepMonitor("[HttpPoster.CallOut]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                FlowTracer.MonitorDetails(() => $"URL : {client.BaseAddress}{api}");

                using var response = await client.GetAsync(api);

                var json = await response.Content.ReadAsStringAsync();

                MessageState state = HttpCodeToMessageState(response.StatusCode);

                FlowTracer.MonitorDetails(() => $"HttpStatus : {response.StatusCode} MessageState : {state} Result : {json}");

                return (state, json);
            }
            catch (HttpRequestException ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                return (MessageState.NetworkError, null);
            }
            finally
            {
                FlowTracer.EndStepMonitor();
            }
        }


        /// <summary>
        /// 访问外部
        /// </summary>
        /// <param name="service">服务名称，用于查找HttpClient，不会与api拼接</param>
        /// <param name="api">完整的接口名称</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public static async Task<(MessageState state, string res)> OutPost(string service, string api, string content)
        {
            if (!HttpClientOption.ServiceMap.TryGetValue(service, out var name))
            {
                name = HttpClientOption.DefaultName;
            }
            FlowTracer.BeginStepMonitor("[HttpPoster.CallOut]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                FlowTracer.MonitorDetails(() => $"URL : {client.BaseAddress}{api}");

                using var httpContent = new StringContent(content);
                using var response = await client.PostAsync(api, httpContent);

                var json = await response.Content.ReadAsStringAsync();

                MessageState state = HttpCodeToMessageState(response.StatusCode);

                FlowTracer.MonitorDetails(() => $"HttpStatus : {response.StatusCode} MessageState : {state} Result : {json}");

                return (state, json);
            }
            catch (HttpRequestException ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                return (MessageState.NetworkError, null);
            }
            finally
            {
                FlowTracer.EndStepMonitor();
            }
        }

        /// <summary>
        /// 访问外部
        /// </summary>
        /// <param name="service">服务名称，用于查找HttpClient，不会与api拼接</param>
        /// <param name="api">完整的接口名称</param>
        /// <param name="forms">Form内容</param>
        /// <returns></returns>
        public static async Task<(MessageState state, string res)> OutFormPost(string service, string api, Dictionary<string, string> forms)
        {
            if (!HttpClientOption.ServiceMap.TryGetValue(service, out var name))
            {
                name = HttpClientOption.DefaultName;
            }
            FlowTracer.BeginStepMonitor("[HttpPoster.OutFormPost]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                FlowTracer.MonitorDetails(() => $"URL : {client.BaseAddress}{api}");
                using var form = new FormUrlEncodedContent(forms);
                using var response = await client.PostAsync(api, form);
                var json = await response.Content.ReadAsStringAsync();

                MessageState state = HttpCodeToMessageState(response.StatusCode);

                FlowTracer.MonitorDetails(() => $"HttpStatus : {response.StatusCode} MessageState : {state} Result : {json}");

                return (state, json);
            }
            catch (HttpRequestException ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => $"发生异常.{ex.Message}");
                return (MessageState.NetworkError, null);
            }
            finally
            {
                FlowTracer.EndStepMonitor();
            }
        }

        #endregion
    }

}
