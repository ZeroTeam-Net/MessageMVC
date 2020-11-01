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
            if (!HttpClientOption.ServiceMap.TryGetValue(message.Topic, out var name))
            {
                name = HttpClientOption.DefaultName;
            }
            IMessageResult result;
            FlowTracer.BeginStepMonitor("[HttpPoster.Post]");
            try
            {
                message.ArgumentOffline();
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                if (client.BaseAddress == null)
                {
                    FlowTracer.MonitorInfomation(() => $"[{message.Topic}/{message.Title}]服务未注册");
                    message.State = MessageState.Unhandled;
                    return null;//直接使用状态
                }
                var uri = new Uri($"{client.BaseAddress }/{message.Topic}/{message.Title}");
                FlowTracer.MonitorDetails(() => $"URL : {uri.OriginalString}");
                using var requestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{client.BaseAddress }/{message.Topic}/{message.Title}"),
                    Method = HttpMethod.Post
                };
                requestMessage.Headers.Add("zeroID", message.ID);
                if (message.Trace != null)
                {
                    requestMessage.Headers.Add("zeroTrace", SmartSerializer.ToInnerString(message.Trace));
                    message.Trace.CallMachine = null;
                }
                if (!string.IsNullOrEmpty(message.Content))
                    requestMessage.Content = new StringContent(message.Content);
                var msg = (MessageItem)message;
                using var response = await client.SendAsync(requestMessage);
                if (requestMessage.Content != null)
                    requestMessage.Content.Dispose();
                var json = await response.Content.ReadAsStringAsync();
                FlowTracer.MonitorDetails(() => $"StatusCode : {response.StatusCode}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    result = new MessageResult
                    {
                        ID = message.ID,
                        State = HttpCodeToMessageState(response.StatusCode)
                    };
                }
                else if (SmartSerializer.TryDeserialize<MessageResult>(json, out var re2))
                {
                    result = re2;
                    re2.DataState = MessageDataState.ResultOffline;
                    if (response.Headers.TryGetValues("zeroState", out var state))
                    {
                        result.State = Enum.Parse<MessageState>(state.FirstOrDefault());
                    }
                }
                else
                {
                    result = new MessageResult
                    {
                        ID = message.ID,
                        State = HttpCodeToMessageState(response.StatusCode)
                    };
                }
                message.State = result.State;
                FlowTracer.MonitorDetails(() => $"State : {result.State} Result : {result.Result}");
                return result;
            }
            catch (HttpRequestException ex)
            {
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
                message.State = MessageState.Unsend;
                return null;//直接使用状态
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
                message.State = MessageState.NetworkError;
                return null;//直接使用状态
            }
            finally
            {
                FlowTracer.EndStepMonitor();
            }
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
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
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
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
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
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorInfomation(() => $"发生异常.{ex.Message}");
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
