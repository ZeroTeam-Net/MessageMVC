using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// Http消息体读取器
    /// </summary>
    public class HttpMessageReader
    {
        /// <summary>
        ///     请求的内容
        /// </summary>
        HttpContext HttpContext;

        /// <summary>
        /// 消息体
        /// </summary>
        HttpMessage Message;


        /// <summary>
        ///     调用检查
        /// </summary>
        /// <param name="context"></param>
        public async Task<(bool success, IInlineMessage message)> CheckRequest(HttpContext context)
        {
            HttpContext = context;
            var request = context.Request;
            //ITokenResolver
            if (!TryGetHeader(request, "x-zmvc-ver", out var ver))
            {
                Message = new HttpMessage
                {
                    IsOutAccess = true,
                    HttpContext = context,
                    Uri = request.Path.Value,
                    HttpMethod = request.Method.ToUpper(),
                    ID = Guid.NewGuid().ToString("N").ToUpper()
                };
                if (!CheckApiRoute())
                {
                    return (false, null);
                }
                CheckTrace();
                await Prepare();
                return (true, Message);
            }
            var content = await ReadContent();
            FlowTracer.MonitorDebug(content);
            if (ver != "v2")
            {
                var message = SmartSerializer.FromInnerString<InlineMessage>(content);
                message.DataState = MessageDataState.ArgumentOffline;
                return (true, message);
            }
            Message = new HttpMessage
            {
                ID = GetHeader(context.Request, "x-zmvc-id"),
                HttpContext = context,
                Uri = request.Path.Value,
                HttpMethod = request.Method.ToUpper(),
                Argument = content,
                HttpContent = content,
                ExtensionDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                DataState = MessageDataState.ArgumentOffline
            };
            if (!CheckApiRoute())
            {
                return (false, null);
            }
            if (TryGetHeader(context.Request, "x-zmvc-trace", out var trace))
            {
                Message.TraceInfo = SmartSerializer.ToObject<TraceInfo>(trace);
            }
            if (TryGetHeader(context.Request, "x-zmvc-user", out var user))
            {
                var bytes = Convert.FromBase64String(user);
                Message.User = SmartSerializer.MsJson.ToObject<Dictionary<string, string>>(bytes);
            }
            if (TryGetHeader(context.Request, "x-zmvc-ctx", out var ctx))
            {
                Message.Context = SmartSerializer.ToObject<Dictionary<string, string>>(ctx);
            }
            return (true, Message);
        }

        private static string GetHeader(HttpRequest request, string name)
        {
            if (!request.Headers.TryGetValue(name, out var head))
            {
                return null;
            }
            return head.First();
        }
        private static bool TryGetHeader(HttpRequest request, string name, out string value)
        {
            if (!request.Headers.TryGetValue(name, out var head))
            {
                value = null;
                return false;
            }
            value = head.First();
            return !string.IsNullOrEmpty(value);
        }
        private void GetHeaderAndSet(HttpRequest request, string name, Action<string> action)
        {
            if (!request.Headers.TryGetValue(name, out var head))
            {
                action(null);
            }
            else
            {
                var value = head.First();
                if (string.IsNullOrEmpty(value))
                    action(null);
                else
                    action(value);
            }
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckApiRoute()
        {
            var words = Message.Uri.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                return false;
            }
            MessageRouteOption.Instance.HostPaths.TryGetValue(words[0], out var idx);
            if (words.Length <= idx + 1)
            {
                return false;
            }
            Message.Service = words[idx].ToLower();
            Message.Method = string.Join('/', words.Skip(idx + 1).Select(p => p.ToLower()));
            return true;
        }

        /// <summary>
        /// 准备在线(框架内调用)
        /// </summary>
        /// <returns></returns>
        async Task Prepare()
        {
            try
            {
                Message.ExtensionDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var request = HttpContext.Request;
                if (request.QueryString.HasValue)
                    ReadArgument(request);
                if (request.HasFormContentType)
                {
                    ReadForm(request);
                    await ReadFiles(request);
                }
                Message.Argument = Message.HttpContent ??= await ReadContent();
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                Message.State = MessageState.FormalError;
            }
            Message.DataState = MessageDataState.ArgumentOffline | MessageDataState.ExtensionInline | MessageDataState.ExtensionOffline;
        }

        private void CheckTrace()
        {
            HttpRequest request = HttpContext.Request;

            Message.TraceInfo = new TraceInfo
            {
                Option = ZeroAppOption.Instance.GetTraceOption(Message.Service),
                Start = DateTime.Now,
                TraceId = Message.ID,
                LocalId = Message.ID,
            };
            if (Message.TraceInfo.Option.HasFlag(MessageTraceType.Request))
            {
                GetHeaderAndSet(request, "x-zmvc-app", app => Message.TraceInfo.RequestApp = app);
                GetHeaderAndSet(request, "Referer", referer => Message.TraceInfo.RequestHost = referer?.ToLower());
                GetHeaderAndSet(request, "x-zmvc-page", app => Message.TraceInfo.RequestPage = app);
            }
            if (Message.TraceInfo.Option.HasFlag(MessageTraceType.LinkTrace))
            {
                Message.TraceInfo.CallMachine = $"{HttpContext.Connection.RemoteIpAddress}:{HttpContext.Connection.RemotePort}";
                Message.TraceInfo.CallId = HttpContext.Connection.Id;
            }
            GetHeaderAndSet(request, "Authorization", token =>
            {
                if (token.IsMissing())
                {
                    token = request.Cookies["access_token"];
                }
                else
                {
                    token = token.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Last();
                }
                Message.TraceInfo.Token = token.IsMissing() || token.Length < 12 ? null : token;
            });

            if (Message.TraceInfo.Option.HasFlag(MessageTraceType.Headers))
            {
                Message.TraceInfo.Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var head in request.Headers)
                {
                    Message.TraceInfo.Headers.Add(head.Key.ToUpper(), head.Value.ToList());
                }
            }
        }

        async Task<string> ReadContent()
        {
            var request = HttpContext.Request;
            if (request.ContentLength != null && request.ContentLength > 0)
            {
                using var texter = new StreamReader(request.Body);
                return await texter.ReadToEndAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        private void ReadForm(HttpRequest request)
        {
            try
            {
                foreach (var key in request.Form.Keys)
                {
                    Message.ExtensionDictionary.TryAdd(key, request.Form[key]);
                }
            }
            catch
            {
            }
        }
        private void ReadArgument(HttpRequest request)
        {
            try
            {
                foreach (var key in request.Query.Keys)
                {
                    Message.ExtensionDictionary.TryAdd(key, request.Query[key]);
                }
            }
            catch
            {
            }
        }

        private async Task ReadFiles(HttpRequest request)
        {
            try
            {
                var files = request.Form?.Files;
                if (files == null || files.Count <= 0)
                {
                    return;
                }
                Message.BinaryDictionary = new Dictionary<string, byte[]>();
                foreach (var file in files)
                {
                    var bytes = new byte[file.Length];
                    using (var stream = file.OpenReadStream())
                    {
                        await stream.ReadAsync(bytes, 0, (int)file.Length);
                    }
                    Message.BinaryDictionary.TryAdd(file.Name, bytes);
                }
            }
            catch
            {
            }
        }
    }
}