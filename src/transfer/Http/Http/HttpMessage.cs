using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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
    [DataContract]
    public class HttpMessage : IMessageItem
    {

        #region IMessageItem

        /// <summary>
        /// 唯一标识
        /// </summary>
        [JsonIgnore]
        public string ID { get; set; }

        /// <summary>
        /// 其他带外内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Extend
        {
            get => HttpContent == null ? null : JsonHelper.SerializeObject(Arguments);
            set => HttpContent = value;
        }

        /// <summary>
        /// 其他带外内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Content => HttpContent ?? JsonHelper.SerializeObject(Arguments);

        string IMessageItem.Topic { get => ApiHost; set => ApiHost = value; }

        string IMessageItem.Title { get => ApiName; set => ApiName = value; }

        string IMessageItem.Content { get => Content; set => HttpContent = value; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public TraceInfo Trace { get; set; }

        /// <summary>
        /// 扩展的二进制
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Binary { get; set; }

        #endregion

        #region Request

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [JsonProperty("Topic", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ApiHost { get; internal set; }

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [JsonProperty("Title", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ApiName { get; internal set; }

        /// <summary>
        ///     请求的内容
        /// </summary>
        public string HttpContent { get; set; }

        /// <summary>
        ///     请求地址
        /// </summary>
        [JsonProperty("uri", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Uri { get; private set; }

        /// <summary>
        ///     HTTP method
        /// </summary>
        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string HttpMethod { get; private set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        public Dictionary<string, string> Arguments { get; set; }

        /// <summary>
        /// 取参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => Arguments == null ? null : Arguments.TryGetValue(key, out var vl) ? vl : null;

        #endregion

        #region Response

        /// <summary>
        ///     文件
        /// </summary>
        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, byte[]> Files;

        /// <summary>
        ///     执行HTTP重写向吗
        /// </summary>
        [JsonProperty("redirect", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool Redirect;

        /// <summary>
        ///     返回文本值
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Result { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     返回二进制值
        /// </summary>
        [JsonProperty("binary", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public byte[] ResultBinary;

        /// <summary>
        ///     返回值是否文件
        /// </summary>
        [JsonProperty("isFile", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsFile;

        #endregion

        #region State

        /// <summary>
        /// 处理状态
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public MessageState State { get; set; }

        /// <summary>
        ///     是否正常
        /// </summary>
        
        [JsonProperty("succeed", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsSucceed => State == MessageState.Success;

        /// <summary>
        ///     开始时间
        /// </summary>
        
        [JsonProperty("start", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime Start { get; set; } = DateTime.Now;

        /// <summary>
        ///     结束时间
        /// </summary>
        
        [JsonProperty("end", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime End { get; set; }

        /// <summary>
        /// 刷新操作
        /// </summary>
        public void Reset()
        {
            End = DateTime.Now;
        }
        /*// <summary>
        ///     执行状态
        /// </summary>
        [JsonProperty("status")] public UserOperatorStateType UserState;

        /// <summary>
        ///     当前请求调用的API配置
        /// </summary>
        public ApiItem ApiItem;

        /// <summary>
        ///     路由主机信息
        /// </summary>
        public RouteHost RouteHost;

        /// <summary>
        ///     结果状态
        /// </summary>
        public ZeroOperatorStateType ZeroState { get; set; }

        /// <summary>
        ///     当前适用的缓存设置对象
        /// </summary>
        public ApiCacheOption CacheSetting;*/

        #endregion

        #region 参数解析

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isBaseValue"></param>
        public string GetArgument(string name, bool isBaseValue)
        {
            if (isBaseValue && Arguments != null && Arguments.TryGetValue(name, out var value))
                return value;
            return HttpContent ?? JsonHelper.SerializeObject(Arguments);
        }

        /// <summary>
        ///     调用检查
        /// </summary>
        /// <param name="context"></param>
        public async Task<bool> CheckRequest(HttpContext context)
        {
            ID = Guid.NewGuid().ToString("N").ToUpper();
            HttpContext = context;
            var request = context.Request;
            Uri = request.Path.Value;
            if (!CheckApiRoute())
            {
                return false;
            }
            Trace = TraceInfo.New(ID);
            Trace.CallId = HttpContext.Connection.Id;
            Trace.Ip = HttpContext.Connection.RemoteIpAddress.ToString();
            Trace.Port = HttpContext.Connection.RemotePort;

            HttpMethod = request.Method.ToUpper();
            CheckHeaders(context, request);
            if (!await ReadArgument(context))
                return false;
            Trace.TraceId = $"{Trace.Token ?? HttpContext.Connection.Id}:{RandomCode.Generate(6)}";
            return true;
        }

        private void CheckHeaders(HttpContext context, HttpRequest request)
        {
            if (HttpRoute.Option.EnableAuthToken)
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
            if (HttpRoute.Option.EnableHeader)
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
                //UserState = UserOperatorStateType.FormalError;
                //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Result = ApiResultHelper.ArgumentErrorJson;
                return false;
            }
            var idx = 0;
            HttpRoute.Option.HostPaths?.TryGetValue(words[0], out idx);
            if (words.Length <= idx + 1)
            {
                //UserState = UserOperatorStateType.FormalError;
                //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Result = ApiResultHelper.ArgumentErrorJson;
                return false;
            }
            ApiHost = words[idx];
            ApiName = string.Join('/', words.Skip(idx + 1));
            return true;
        }

        private async Task<bool> ReadArgument(HttpContext context)
        {
            var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var request = context.Request;
            try
            {
                if (request.QueryString.HasValue)
                {
                    foreach (var key in request.Query.Keys)
                    {
                        arguments.TryAdd(key, request.Query[key]);
                    }
                }
                if (request.HasFormContentType)
                {
                    foreach (var key in request.Form.Keys)
                    {
                        arguments.TryAdd(key, request.Form[key]);
                    }

                    if (!await ReadFiles(request))
                    {
                        return false;
                    }
                }

                if (request.ContentLength != null && request.ContentLength > 0)
                {
                    using var texter = new StreamReader(request.Body);
                    HttpContent = await texter.ReadToEndAsync();
                    if (string.IsNullOrEmpty(HttpContent))
                    {
                        HttpContent = null;
                    }
                    texter.Close();
                }
                if (arguments.Count > 0)
                    Arguments = arguments;
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "读取远程参数");
                //UserState = UserOperatorStateType.FormalError;
                //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Result = ApiResultHelper.ArgumentErrorJson;
                return false;
            }
        }

        private async Task<bool> ReadFiles(HttpRequest request)
        {
            if (!HttpRoute.Option.EnableFormFile)
            {
                return true;
            }
            var files = request.Form?.Files;
            if (files == null || files.Count <= 0)
            {
                return true;
            }
            Files = new Dictionary<string, byte[]>();
            foreach (var file in files)
            {
                if (Files.ContainsKey(file.Name))
                {
                    //UserState = UserOperatorStateType.FormalError;
                    //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    Result = ApiResultHelper.ArgumentErrorJson;
                    return false;
                }

                var bytes = new byte[file.Length];
                await using (var stream = file.OpenReadStream())
                {
                    await stream.ReadAsync(bytes, 0, (int)file.Length);
                }
                Files.Add(file.Name, bytes);
            }
            return true;
        }
        #endregion
    }
}