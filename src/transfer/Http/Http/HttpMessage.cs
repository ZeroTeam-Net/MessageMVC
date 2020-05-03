using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class HttpMessage : MessageItem, IInlineMessage
    {

        #region IMessageItem

        /// <summary>
        /// 是否外部访问
        /// </summary>
        public bool IsOutAccess => true;

        /// <summary>
        /// 实体参数
        /// </summary>
        public object ArgumentData { get; set; }

        /// <summary>
        /// 字典参数
        /// </summary>
        public Dictionary<string, string> Dictionary { get; set; }

        /// <summary>
        /// 数据状态
        /// </summary>
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
        public object ResultData
        {
            get => resultData;
            set
            {
                resultData = value;
                DataState |= MessageDataState.ResultInline;
                if (Result == null && value == null && runtimeStatus == null)
                    DataState |= MessageDataState.ResultOffline;
                else
                    DataState &= ~MessageDataState.ResultOffline;
            }
        }

        private IOperatorStatus runtimeStatus;

        /// <summary>
        /// 执行状态
        /// </summary>
        public IOperatorStatus RuntimeStatus
        {
            get => runtimeStatus;
            set
            {
                runtimeStatus = value;
                DataState |= MessageDataState.ResultInline;
                if (Result == null && value == null && resultData == null)
                    DataState |= MessageDataState.ResultOffline;
                else
                    DataState &= ~MessageDataState.ResultOffline;
            }
        }

        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        public ISerializeProxy ResultSerializer { get; set; }


        /// <summary>
        ///     返回值构造对象
        /// </summary>
        public Func<int, string, object> ResultCreater { get; set; }


        #endregion

        #region Request

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// 服务名称,即Topic
        /// </summary>
        public string ServiceName { get => Topic; set => Topic = value; }

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        public string ApiHost { get => Topic; internal set => Topic = value; }

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        public string ApiName { get => Title; internal set => Title = value; }

        /// <summary>
        ///     请求地址
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        ///     HTTP method
        /// </summary>
        public string HttpMethod { get; private set; }

        /// <summary>
        /// 接口参数,即Content
        /// </summary>
        public string Argument { get => Content; set => Content = value; }

        /// <summary>
        ///     请求的内容
        /// </summary>
        public string HttpContent { get; set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        public Dictionary<string, string> HttpArguments { get; set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        public Dictionary<string, string> HttpForms { get => Dictionary; set => Dictionary = value; }

        /// <summary>
        ///     请求的内容字典
        /// </summary>
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

        #region 参数解析

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
            ID = Guid.NewGuid().ToString("N").ToUpper();
            Trace = TraceInfo.New(ID);
            Trace.CallId = HttpContext.Connection.Id;
            Trace.CallApp = "Client";
            Trace.CallMachine = $"{HttpContext.Connection.RemoteIpAddress}:{HttpContext.Connection.RemotePort}";

            HttpMethod = request.Method.ToUpper();
            CheckHeaders(context, request);
            //Trace.TraceId = $"{Trace.Token ?? HttpContext.Connection.Id}:{RandomCode.Generate(6)}";
            DataState = MessageDataState.None;
            return true;
        }

        private void CheckHeaders(HttpContext context, HttpRequest request)
        {
            if (MessageRouteOption.Instance.EnableAuthToken)
            {
                Trace.Token = request.Headers["AUTHORIZATION"].LastOrDefault()?
                .Trim()
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                ?? context.Request.Query["token"];
                if (string.IsNullOrWhiteSpace(Trace.Token) ||
                    Trace.Token.Equals("null") ||
                    Trace.Token.Equals("undefined") ||
                    Trace.Token.Equals("Bearer"))
                {
                    Trace.Token = null;
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
                State = MessageState.NonSupport;
                return false;
            }
            MessageRouteOption.Instance.HostPaths.TryGetValue(words[0], out var idx);
            if (words.Length <= idx + 1)
            {
                State = MessageState.NonSupport;
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
            if (HttpForms.Count > 0)
            {
                return serialize.ToString(HttpForms);
            }
            if (HttpArguments.Count > 0)
            {
                return serialize.ToString(HttpArguments);
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
            var value = GetValueArgument(name, scope);
            if (value == null && type != typeof(string))
            {
                throw new MessageArgumentNullException(name);
            }
            return value;
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
            var vl = GetValueArgument(name, scope);
            //return serialize.ToObject(arg, type);
            return vl;
        }

        /// <summary>
        /// 取参数值(动态IL代码调用)
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <returns>值</returns>
        public string GetValueArgument(string name, int scope = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            switch ((ArgumentScope)scope)
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
            if (ContentObject == null)
            {
                ContentObject = string.IsNullOrWhiteSpace(HttpContent)
                    ? new JObject()
                    : (JObject)JsonConvert.DeserializeObject(HttpContent);
            }
            if (ContentObject.TryGetValue(name, out var vl))
                return vl?.ToString();
            if (HttpForms.TryGetValue(name, out var fm))
                return fm?.ToString();
            if (HttpArguments.TryGetValue(name, out var ar))
                return ar;
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
                ResultCreater = errResultCreater;

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
                LogRecorder.Exception(e);
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
                Dictionary = PrepareHttpForm();
                foreach (var kv in HttpArguments)
                {
                    Dictionary.TryAdd(kv.Key, kv.Value);
                }
                ReadFiles();
                HttpContent ??= await PrepareContent();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
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
    }
}