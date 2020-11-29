using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        public async Task<(bool success,IInlineMessage message)> CheckRequest(HttpContext context)
        {
            HttpContext = context;
            var request = context.Request;
            //ITokenResolver
            if (TryGetHeader(request, "x-zmvc-ver", out _))
            {
                var content = await ReadContent();
                FlowTracer.MonitorTrace(content);
                var message = SmartSerializer.FromInnerString<InlineMessage>(content);
                message.Trace ??= TraceInfo.New(message.ID);
                message.DataState = MessageDataState.ArgumentOffline;
                return (true,message);
            }

            Message = new HttpMessage
            {
                IsOutAccess = true,
                HttpContext=context,
                Uri = request.Path.Value,
                HttpMethod = request.Method.ToUpper(),
                ID = Guid.NewGuid().ToString("N").ToUpper()
            };
            if (!CheckApiRoute())
            {
                return (false,null);
            }
            CheckTrace();
            await Prepare();
            return (true,Message);
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
            Message.ApiHost = words[idx].ToLower();
            Message.ApiName = string.Join('/', words.Skip(idx + 1).Select(p=>p.ToLower()));
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
                ReadArgument();
                ReadForm();
                ReadFiles();
                Message.Content = Message.HttpContent ??= await ReadContent();
            }
            catch (Exception e)
            {
                DependencyScope.Logger.Exception(e);
                Message.State = MessageState.FormalError;
            }
            Message.DataState = MessageDataState.ArgumentOffline | MessageDataState.ExtensionInline | MessageDataState.ExtensionOffline;
        }

        private void CheckTrace()
        {
            HttpRequest request = HttpContext.Request;
            Message.Trace = TraceInfo.New(Message.ID);
            if (ZeroAppOption.Instance.TraceInfo.HasFlag(TraceInfoType.App))
            {
                GetHeaderAndSet(request, "x-zmvc-app", app => Message.Trace.RequestApp = app);
                GetHeaderAndSet(request, "Referer", referer => Message.Trace.RequestPage = referer?.ToLower());
            }
            if (ZeroAppOption.Instance.TraceInfo.HasFlag(TraceInfoType.LinkTrace))
            {
                Message.Trace.CallMachine = $"{HttpContext.Connection.RemoteIpAddress}:{HttpContext.Connection.RemotePort}";
                Message.Trace.CallId = HttpContext.Connection.Id;
            }
            GetHeaderAndSet(request, "Authorization", token => Message.Trace.Token = token);

            if (ZeroAppOption.Instance.TraceInfo.HasFlag(TraceInfoType.Headers))
            {
                Message.Trace.Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var head in request.Headers)
                {
                    Message.Trace.Headers.Add(head.Key.ToUpper(), head.Value.ToList());
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

        private void ReadForm()
        {
            var request = HttpContext.Request;
            try
            {
                if (request.HasFormContentType)
                {
                    foreach (var key in request.Form.Keys)
                    {
                        Message.ExtensionDictionary.TryAdd(key, request.Form[key]);
                    }
                }
            }
            catch
            {
            }
        }
        private void ReadArgument()
        {
            var request = HttpContext.Request;
            try
            {
                if (request.QueryString.HasValue)
                {
                    foreach (var key in request.Query.Keys)
                    {
                        //if (key.Length == 1 && key[0] == '_')
                        //    continue;
                        //if (key.Length >= 2 && key[0] == '_' && key[1] == '_')
                        //    continue;
                        Message.ExtensionDictionary.TryAdd(key, request.Query[key]);
                    }
                }
            }
            catch
            {
            }
        }

        private void ReadFiles()
        {
            //if (!MessageRouteOption.Instance.EnableFormFile)
            //{
            //    return;
            //}
            //Binary = new Dictionary<string, byte[]>();
            //var request = HttpContext.Request;
            //var files = request.Form?.Files;
            //if (files == null || files.Count <= 0)
            //{
            //    return;
            //}
            //foreach (var file in files)
            //{
            //    var bytes = new byte[file.Length];
            //    using (var stream = file.OpenReadStream())
            //    {
            //        stream.Read(bytes, 0, (int)file.Length);
            //    }
            //    Binary.TryAdd(file.Name, bytes);
            //}
        }
    }
}