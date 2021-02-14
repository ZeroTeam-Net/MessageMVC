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
    public class HttpPoster : MessagePostBase, IMessagePoster, IZeroDiscover
    {
        #region IMessagePoster

        /// <summary>
        ///  发现
        /// </summary>
        Task IZeroDiscover.Discovery()
        {
            Logger.Information($"{nameof(HttpPoster)}已开启");
            return Task.CompletedTask;
        }

        ILifeFlow IMessagePoster.GetLife() => null;

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
            using var _ = FlowTracer.DebugStepScope("[HttpPoster.Post]");
            StringContent content = null;
            try
            {
                ZeroAppOption.Instance.BeginRequest();
                message.ArgumentOffline();
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                if (client.BaseAddress == null)
                {
                    client.BaseAddress = new Uri(HttpClientOption.Instance.DefaultUrl);
                    client.Timeout = TimeSpan.FromSeconds(HttpClientOption.Instance.DefaultTimeOut);
                    FlowTracer.MonitorError(() => $"[{message.Service}/{message.Method}]服务未注册,使用默认地址：{HttpClientOption.Instance.DefaultUrl}");
                    //message.State = MessageState.Unhandled;
                    //return null;//直接使用状态
                }
                var uri = new Uri($"{client.BaseAddress }{message.Service}/{message.Method}");
                FlowTracer.MonitorDetails(() =>
                {
                    return $"URL : {uri.OriginalString}";
                });

                using var requestMessage = new HttpRequestMessage
                {
                    RequestUri = uri,
                    Method = HttpMethod.Post
                };
                if (message.Argument.IsPresent())
                {
                    requestMessage.Content = content = new StringContent(message.Argument);
                }
                requestMessage.Headers.Add("x-zmvc-ver", "v2");
                requestMessage.Headers.Add("x-zmvc-id", message.ID);
                if (message.TraceInfo != null )
                    requestMessage.Headers.Add("x-zmvc-trace", message.TraceInfo.ToInnerString());
                if (message.User != null && message.User.Count > 0)
                    requestMessage.Headers.Add("x-zmvc-user", Convert.ToBase64String(SmartSerializer.ToBytes(message.User)));
                if (message.Context != null && message.Context.Count > 0)
                    requestMessage.Headers.Add("x-zmvc-ctx", message.Context.ToInnerString());

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
                ZeroAppOption.Instance.EndRequest();
                content?.Dispose();
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
            using var _ = FlowTracer.DebugStepScope("[HttpPoster.OutGet]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(service);
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
        }


        /// <summary>
        /// 访问外部
        /// </summary>
        /// <param name="service">服务名称，用于查找HttpClient，不会与api拼接</param>
        /// <param name="api">完整的接口名称</param>
        /// <param name="argument">内容</param>
        /// <returns></returns>
        public static async Task<(MessageState state, string res)> OutPost(string service, string api, string argument)
        {
            using var _ = FlowTracer.DebugStepScope("[HttpPoster.CallOut]");

            HttpStatusCode httpStatusCode;
            StringContent content = null;
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(service);
                FlowTracer.MonitorDetails(() => $"URL : {client.BaseAddress}{api}");
                string json;
                content = new StringContent(argument ?? "");
                using var response = await client.PostAsync(api, content);
                httpStatusCode = response.StatusCode;
                json = await response.Content.ReadAsStringAsync();
                var state = HttpCodeToMessageState(httpStatusCode);
                FlowTracer.MonitorDetails(() => $"HttpStatus : {httpStatusCode} MessageState : {state} Result : {json}");
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
                content?.Dispose();
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
            using var  _ =FlowTracer.DebugStepScope("[HttpPoster.OutFormPost]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(service);
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
        }

        #endregion
    }

}
