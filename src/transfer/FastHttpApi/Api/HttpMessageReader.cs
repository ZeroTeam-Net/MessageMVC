using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using BeetleX.FastHttpApi.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// Http消息体读取器
    /// </summary>
    public class HttpMessageReader
    {

        #region 消息接收与处理

        /// <summary>
        /// Http消息接收处理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public static bool OnHttpRequest(HttpApiServer server, HttpRequest request, HttpResponse response)
        {
            if (Path.GetFileName(request.BaseUrl).Contains('.'))
            {
                return false;
            }
            var writer = new HttpWriter
            {
                Request = request,
                Response = response
            };
            try
            {
                //命令
                var reader = new HttpMessageReader();
                var (success, message) = reader.CheckRequest(server, request, response);
                //开始调用
                if (success)
                {
                    var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                    {
                        ServiceName = message.Service,
                        Receiver = new EmptyReceiver(),
                        Serialize = DependencyHelper.GetService<ISerializeProxy>()
                    };
                    MessageProcessor.RunOnMessagePush(service, message, false, writer);
                }
                else
                {
                    writer.WriteResult(ApiResultHelper.State(OperatorStatusCode.NoFind));
                }
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                writer.WriteResult(ApiResultHelper.State(OperatorStatusCode.BusinessError));
            }
            return true;
        }
        #endregion

        /// <summary>
        ///     请求的内容
        /// </summary>
        HttpRequest Request;

        /// <summary>
        /// 消息体
        /// </summary>
        public HttpMessage Message;


        /// <summary>
        ///     调用检查
        /// </summary>
        public (bool success, IInlineMessage message) CheckRequest(HttpApiServer server, HttpRequest request, HttpResponse response)
        {
            Request = request;

            var ver = Request.Header["x-zmvc-ver"];
            if (!string.IsNullOrEmpty(ver))
            {
                string content;
                using (request.Stream.LockFree())
                {
                    using var streamReader = new StreamReader(request.Stream);
                    content = streamReader.ReadToEnd();
                }
                if (ver != "v2")
                {
                    var message = SmartSerializer.FromInnerString<InlineMessage>(content);
                    message.TraceInfo ??= message.CreateTraceInfo();
                    message.DataState = MessageDataState.ArgumentOffline;
                    return (true, message);
                }
                Message = new HttpMessage
                {
                    ID = Request.Header["x-zmvc-id"],
                    IsOutAccess = true,
                    Request = request,
                    HttpContent = content,
                    Url = request.Url,
                    Argument = content,
                    HttpMethod = request.Method.ToUpper(),
                    DataState = MessageDataState.ArgumentOffline
                };
                if (!CheckApiRoute())
                {
                    return (false, null);
                }
                var trace = Request.Header["x-zmvc-trace"];
                if (!string.IsNullOrEmpty(trace))
                {
                    Message.TraceInfo = SmartSerializer.ToObject<TraceInfo>(trace);
                }
                var user = Request.Header["x-zmvc-user"];
                if (!string.IsNullOrEmpty(user))
                {
                    Message.User = SmartSerializer.ToObject<Dictionary<string, string>>(user);
                }
                var ctx = Request.Header["x-zmvc-ctx"];
                if (!string.IsNullOrEmpty(ctx))
                {
                    Message.Context = SmartSerializer.ToObject<Dictionary<string, string>>(ctx);
                }
            }
            else
            {
                Message = new HttpMessage
                {
                    ID = Guid.NewGuid().ToString("N").ToUpper(),
                    IsOutAccess = true,
                    Request = request,
                    Url = request.Url,
                    HttpMethod = request.Method.ToUpper(),
                    DataState = MessageDataState.ArgumentOffline
                };
                if (!CheckApiRoute())
                {
                    return (false, null);
                }
                CheckTrace();
                Prepare();
            }

            Message.HttpContext = new HttpContext(server, request, response, Message)
            {
                ActionUrl = Message.Url
            };

            Message.ContentObject = string.IsNullOrWhiteSpace(Message.HttpContent)
                    ? new JObject()
                    : (JObject)JsonConvert.DeserializeObject(Message.HttpContent);
            return (true, Message);
        }

        private void GetHeaderAndSet(string name, Action<string> action)
        {
            var value = Request.Header[name];

            if (string.IsNullOrEmpty(value))
                action(null);
            else
                action(value);
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckApiRoute()
        {
            var words = Message.Url.Split('?')[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                return false;
            }
            if (words[0].IsMe("api"))
            {
                if (words.Length < 3)
                {
                    return false;
                }
                Message.Service = words[1].ToLower();
                Message.Method = string.Join('/', words.Skip(2).Select(p => p.ToLower()));
            }
            else
            {
                Message.Service = words[0].ToLower();
                Message.Method = string.Join('/', words.Skip(1).Select(p => p.ToLower()));
            }
            return true;
        }

        /// <summary>
        /// 准备在线(框架内调用)
        /// </summary>
        /// <returns></returns>
        void Prepare()
        {
            Message.ExtensionDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Message.DataState = MessageDataState.ArgumentOffline | MessageDataState.ExtensionInline | MessageDataState.ExtensionOffline;

            ReadArgument();

            if (!string.IsNullOrEmpty(Request.ContentType))
            {
                if (Request.ContentType.Contains("application/x-www-form-urlencoded", StringComparison.CurrentCultureIgnoreCase))
                {
                    var convert = new FormUrlDataConvertAttribute();
                    convert.Execute(Message, Request);
                    return;
                }
                else if (Request.ContentType.Contains("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
                {
                    var convert = new MultiDataConvertAttribute();
                    convert.Execute(Message, Request);
                    return;
                }
            }

            using (Request.Stream.LockFree())
            {
                using var streamReader = new StreamReader(Request.Stream);
                Message.Argument = Message.HttpContent = streamReader.ReadToEnd();
            }
        }


        private void CheckTrace()
        {
            HttpRequest request = Request;
            Message.TraceInfo = Message.CreateTraceInfo();
            if (Message.TraceInfo.Option.HasFlag(MessageTraceType.Request))
            {
                GetHeaderAndSet("x-zmvc-app", app => Message.TraceInfo.RequestApp = app);
                GetHeaderAndSet("Referer", referer => Message.TraceInfo.RequestPage = referer?.ToLower());
            }
            if (Message.TraceInfo.Option.HasFlag(MessageTraceType.LinkTrace))
            {
                Message.TraceInfo.CallMachine = $"{request.Session.RemoteEndPoint}:{request.Session.Port}";
                Message.TraceInfo.CallId = request.ID.ToString();
            }
            GetHeaderAndSet("Authorization", token => Message.TraceInfo.Token = token);

            if (Message.TraceInfo.Option.HasFlag(MessageTraceType.Headers))
            {
                Message.TraceInfo.Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var head in request.Header.Copy())
                {
                    Message.TraceInfo.Headers.Add(head.Key.ToUpper(), new List<string> { head.Value });
                }
            }
        }

        private void ReadArgument()
        {
            var sp = Request.Url.Split('?');
            if (sp.Length <= 1)
                return;
            var items = sp[1].Split('&');
            foreach (var item in items)
            {
                var words = item.Split('=');
                if (words.Length > 1)
                    Message.ExtensionDictionary.TryAdd(words[0], words[1]);
                else
                    Message.ExtensionDictionary.TryAdd(words[0], null);
            }
        }

    }
}
