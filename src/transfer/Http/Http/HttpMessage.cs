using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HttpMessage : MessageItem, IInlineMessage
    {
        #region IMessageItem

        /// <summary>
        /// 是否外部访问
        /// </summary>
        [JsonIgnore]
        public bool IsOutAccess { get; set; }

        /// <summary>
        /// 实体参数
        /// </summary>
        [JsonIgnore]
        public object ArgumentData { get; set; }

        /// <summary>
        /// 字典参数
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> Dictionary { get; set; }

        /// <summary>
        /// 数据状态
        /// </summary>
        [JsonIgnore]
        public MessageDataState DataState { get; set; }

        private object resultData;
        /// <summary>
        /// 处理结果,对应状态的解释信息
        /// </summary>
        /// <remarks>
        /// 未消费:无内容
        /// 已接受:无内容
        /// 格式错误 : 无内容
        /// 无处理方法 : 无内容
        /// 处理异常 : 异常信息
        /// 处理失败 : 失败内容或原因
        /// 处理成功 : 结果信息或无
        /// </remarks>
        [JsonIgnore]
        public object ResultData
        {
            get => resultData;
            set
            {
                resultData = value;
                DataState |= MessageDataState.ResultInline;
                if (Result == null && value == null)
                    DataState |= MessageDataState.ResultOffline;
                else
                    DataState &= ~MessageDataState.ResultOffline;
            }
        }

        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        [JsonIgnore]
        public ISerializeProxy ResultSerializer { get; set; }


        /// <summary>
        ///     返回值构造对象
        /// </summary>
        [JsonIgnore]
        public Func<int, string, object> ResultCreater { get; set; }


        #endregion

        #region Request

        /// <summary>
        ///     Http上下文
        /// </summary>
        [JsonIgnore]
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// 服务名称,即Topic
        /// </summary>
        [JsonIgnore]
        public string ServiceName { get => Topic; set => Topic = value; }

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [JsonIgnore]
        public string ApiHost { get => Topic; internal set => Topic = value; }

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [JsonIgnore]
        public string ApiName { get => Title; internal set => Title = value; }

        /// <summary>
        ///     请求地址
        /// </summary>
        [JsonIgnore]
        public string Uri { get; private set; }

        /// <summary>
        ///     HTTP method
        /// </summary>
        [JsonIgnore]
        public string HttpMethod { get; private set; }

        /// <summary>
        /// 接口参数,即Content
        /// </summary>
        [JsonIgnore]
        public string Argument { get => Content; set => Content = value; }

        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonIgnore]
        public string HttpContent { get; set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> HttpArguments { get; set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> HttpForms { get => Dictionary; set => Dictionary = value; }

        /// <summary>
        ///     请求的内容字典
        /// </summary>
        [JsonIgnore]
        public JObject ContentObject { get; set; }

        #endregion

        #region State

        /// <summary>
        ///     开始时间
        /// </summary>

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime? Start { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime? End { get; set; }

        #endregion

        #region 请求解析

        /// <summary>
        ///     调用检查
        /// </summary>
        /// <param name="context"></param>
        public bool CheckRequest(HttpContext context)
        {
            HttpContext = context;
            var request = context.Request;
            Uri = request.Path.Value;
            if (!CheckApiRoute())
            {
                return false;
            }
            if(request.Headers.TryGetValue("zeroID",out var vl))
            {
                IsOutAccess = false;
                ID = vl[0];
                if(request.Headers.TryGetValue("zeroTrace", out vl))
                {
                    Trace = SmartSerializer.ToObject<TraceInfo>(vl[0]);
                }
                else
                {
                    Trace = TraceInfo.New(ID);
                    Trace.CallId = HttpContext.Connection.Id;
                }
            }
            else
            {
                IsOutAccess = true;
                HttpMethod = request.Method.ToUpper();
                ID = Guid.NewGuid().ToString("N").ToUpper();
                Trace = TraceInfo.New(ID);
                Trace.CallId = HttpContext.Connection.Id;
                CheckHeaders(request);
            }
            Trace.CallMachine = $"{HttpContext.Connection.RemoteIpAddress}:{HttpContext.Connection.RemotePort}";
            DataState = MessageDataState.None;

            return true;
        }

        private void CheckHeaders(HttpRequest request)
        {
            var referer = request.Headers["Referer"].LastOrDefault();
            if (referer != null)
            {
                var uri = new UriBuilder(referer);
                Trace.CallPage = uri.Path;
            }
            if (MessageRouteOption.Instance.EnableAuthToken)
            {
                var header = request.Headers["Authorization"];
                if (header.Count > 0)
                {
                    var token = header[0]?.Trim().Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (string.IsNullOrWhiteSpace(token) ||
                        token.Equals("null") ||
                        token.Equals("undefined") ||
                        token.Equals("Bearer"))
                    {
                        Trace.Token = null;
                    }
                    else
                    {
                        var ks = token.Split('|');
                        Trace.Token = ks[0];
                        if (ks.Length > 1)
                            Trace.CallApp = ks[1];
                        if (ks.Length > 2)
                            Trace.CallId = ks[2];
                    }
                }
            }
            if (MessageRouteOption.Instance.EnableHeader)
            {
                Trace.Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var head in request.Headers)
                {
                    Trace.Headers.Add(head.Key.ToUpper(), head.Value.ToList());
                }
            }
            Trace.CallApp ??= request.Query["__app"];
            Trace.CallPage ??= request.Query["__page"];
            Trace.Token ??= request.Query["__token"];
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckApiRoute()
        {
            var words = Uri.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                State = MessageState.Unhandled;
                return false;
            }
            MessageRouteOption.Instance.HostPaths.TryGetValue(words[0], out var idx);
            if (words.Length <= idx + 1)
            {
                State = MessageState.Unhandled;
                return false;
            }
            ApiHost = words[idx];
            ApiName = string.Join('/', words.Skip(idx + 1));
            return true;
        }

        #endregion

        #region Inline

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        public object GetArgument(int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            var str = GetContent((ArgumentScope)scope, ref serialize);
            if (string.IsNullOrEmpty(str))
                return null;
            return serialize.ToObject(str, type);
        }

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="scope">范围</param>
        /// <param name="serialize">序列化类型</param>
        /// <returns>值</returns>
        string GetContent(ArgumentScope scope, ref ISerializeProxy serialize)
        {
            serialize ??= ResultSerializer;
            switch (scope)
            {
                case ArgumentScope.HttpArgument:
                    if (HttpArguments.Count > 0)
                        return serialize.ToString(HttpArguments);
                    return null;
                case ArgumentScope.HttpForm:
                    if (HttpForms.Count > 0)
                        return serialize.ToString(HttpForms);
                    return null;
            }
            if (!string.IsNullOrEmpty(HttpContent))
                return HttpContent;
            if (Dictionary.Count > 0)
            {
                return serialize.ToString(Dictionary);
            }
            return null;
        }

        /// <summary>
        /// 取参数值(动态IL代码调用)  BUG
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        public object FrameGetValueArgument(string name, int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            var val = GetScopeArgument(name, (ArgumentScope)scope);

            if (val == null && type != typeof(string))
            {
                throw new MessageArgumentNullException(name);
            }
            return val;
        }

        /// <summary>
        /// 取参数值(动态IL代码调用)  BUG
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        public object GetValueArgument(string name, int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            return GetScopeArgument(name, (ArgumentScope)scope);
        }

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <returns>值</returns>
        public string GetScopeArgument(string name, ArgumentScope scope = ArgumentScope.HttpArgument)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            switch (scope)
            {
                case ArgumentScope.HttpArgument:
                    if (HttpArguments.TryGetValue(name, out var ha))
                        return ha;
                    return null;
                case ArgumentScope.HttpForm:
                    if (HttpForms.TryGetValue(name, out var af))
                        return af?.ToString();
                    return null;
            }
            if (Dictionary.TryGetValue(name, out var fm))
                return fm?.ToString();
            if (HttpForms.TryGetValue(name, out fm))
                return fm?.ToString();
            if (HttpArguments.TryGetValue(name, out var ar))
                return ar;

            ContentObject ??= string.IsNullOrWhiteSpace(HttpContent)
                    ? new JObject()
                    : (JObject)JsonConvert.DeserializeObject(HttpContent);

            if (ContentObject.TryGetValue(name, out var vl))
                return vl?.ToString();
            return null;
        }

        #endregion

        #region 状态

        /// <summary>
        /// 如果未上线且还原参数为字典,否则什么也不做
        /// </summary>
        public Task ArgumentInline(ISerializeProxy serialize, Type type, ISerializeProxy resultSerializer, Func<int, string, object> errResultCreater)
        {
            if (resultSerializer != null)
                ResultSerializer = resultSerializer;
            if (errResultCreater != null)
                ResultCreater = errResultCreater;//BUG

            try
            {
                if (type == null || type.IsBaseType())
                {
                    //使用Form与Arguments组合的字典对象
                    DataState |= MessageDataState.ArgumentInline;
                    DataState &= ~MessageDataState.ArgumentOffline;

                    var dir = SmartSerializer.ToObject<Dictionary<string, string>>(HttpContent);
                    if (dir != null)
                    {
                        foreach (var item in dir)
                            Dictionary[item.Key] = item.Value;
                    }
                }
                else
                {
                    Content = !string.IsNullOrEmpty(HttpContent)
                         ? HttpContent
                         : SmartSerializer.ToString(Dictionary);
                    DataState |= MessageDataState.ArgumentOffline;

                    ArgumentData = SmartSerializer.ToObject(Content, type);
                    DataState |= MessageDataState.ArgumentInline;
                }
            }
            catch (Exception e)
            {
                DependencyScope.Logger.Exception(e);
                State = MessageState.FormalError;
            }
            return Task.CompletedTask;
        }
        #endregion

        #region 原始参数读取

        /// <summary>
        /// 准备在线(框架内调用)
        /// </summary>
        /// <returns></returns>
        public async Task PrepareInline()
        {
            try
            {
                HttpArguments ??= PrepareHttpArgument();
                Dictionary ??= PrepareHttpForm();
                foreach (var kv in HttpArguments)
                {
                    Dictionary.TryAdd(kv.Key, kv.Value);
                }
                ReadFiles();
                HttpContent ??= await PrepareContent();
                Content = HttpContent;
            }
            catch (Exception e)
            {
                DependencyScope.Logger.Exception(e);
                State = MessageState.FormalError;
            }
            DataState &= ~(MessageDataState.ArgumentInline | MessageDataState.ArgumentOffline);
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



        async Task<string> PrepareContent()
        {
            var request = HttpContext.Request;
            if (request.ContentLength != null && request.ContentLength > 0)
            {
                using var texter = new StreamReader(request.Body);
                return await texter.ReadToEndAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        private Dictionary<string, string> PrepareHttpForm()
        {
            var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var request = HttpContext.Request;
            try
            {
                if (request.HasFormContentType)
                {
                    foreach (var key in request.Form.Keys)
                    {
                        arguments.TryAdd(key, request.Form[key]);
                    }
                }
            }
            catch
            {
            }
            return arguments;
        }
        private Dictionary<string, string> PrepareHttpArgument()
        {
            var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
                        arguments.TryAdd(key, request.Query[key]);
                    }
                }
            }
            catch
            {
            }
            return arguments;
        }
        #endregion


        #region 快捷方法
        /// <summary>
        /// 跟踪消息
        /// </summary>
        /// <returns></returns>
        string IInlineMessage.TraceInfo()
        {
            var code = new StringBuilder();
            code.AppendLine($"ID:{ID}");
            code.AppendLine($"URL:{HttpContext.Request.GetDisplayUrl()}");
            code.AppendLine($"Trace:{JsonConvert.SerializeObject(Trace, Formatting.Indented)}");

            if (HttpArguments != null && HttpArguments.Count > 0)
                code.AppendLine($"Arguments:{JsonConvert.SerializeObject(HttpArguments, Formatting.Indented)}");
            if (Dictionary != null && Dictionary.Count > 0)
                code.AppendLine($"Dictionary:{JsonConvert.SerializeObject(Dictionary, Formatting.Indented)}");
            if (HttpContent != null)
                code.AppendLine($"Content:{HttpContent}");

            return code.ToString();
        }

        #endregion
    }
}