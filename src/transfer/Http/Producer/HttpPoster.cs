using Agebull.Common.Logging;
using System;
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
            LogRecorder.BeginStepMonitor("[HttpPoster.Post]");
            try
            {
                message.Offline();
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                LogRecorder.MonitorDetails(() => $"URL : {client.BaseAddress }{message.Topic}/{message.Title}");

                using var content = new StringContent(SmartSerializer.SerializeMessage(message));
                using var response = await client.PostAsync($"/{message.Topic}/{message.Title}", content);

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
                        Trace = message.Trace,
                        State = HttpCodeToMessageState(response.StatusCode)
                    };
                }
                message.State = result.State;
                LogRecorder.MonitorDetails(() => $"State : {result.State} Result : {result.Result}");
                return result;
            }
            catch (HttpRequestException ex)
            {
                LogRecorder.MonitorInfomation(() => $"发生异常.{ex.Message}");
                message.State = MessageState.Unsend;
                return null;//直接使用状态
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

        static MessageState HttpCodeToMessageState(HttpStatusCode httpStatusCode)
        {
            switch (httpStatusCode)
            {
                case HttpStatusCode.OK:
                    return MessageState.NoUs;

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
            LogRecorder.BeginStepMonitor("[HttpPoster.CallOut]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                LogRecorder.MonitorDetails(() => $"URL : {client.BaseAddress}{api}");

                using var response = await client.GetAsync(api);

                var json = await response.Content.ReadAsStringAsync();

                MessageState state = HttpCodeToMessageState(response.StatusCode);

                LogRecorder.MonitorDetails(() => $"HttpStatus : {response.StatusCode} MessageState : {state} Result : {json}");

                return (state, json);
            }
            catch (HttpRequestException ex)
            {
                LogRecorder.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                LogRecorder.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.NetworkError, null);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
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
            LogRecorder.BeginStepMonitor("[HttpPoster.CallOut]");
            try
            {
                var client = HttpClientOption.HttpClientFactory.CreateClient(name);
                LogRecorder.MonitorDetails(() => $"URL : {client.BaseAddress}{api}");

                using var httpContent = new StringContent(content);
                using var response = await client.PostAsync(api, httpContent);

                var json = await response.Content.ReadAsStringAsync();

                MessageState state = HttpCodeToMessageState(response.StatusCode);

                LogRecorder.MonitorDetails(() => $"HttpStatus : {response.StatusCode} MessageState : {state} Result : {json}");

                return (state, json);
            }
            catch (HttpRequestException ex)
            {
                LogRecorder.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.Unsend, null);
            }
            catch (Exception ex)
            {
                LogRecorder.MonitorInfomation(() => $"发生异常.{ex.Message}");
                return (MessageState.NetworkError, null);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
        }

        #endregion
    }

}
