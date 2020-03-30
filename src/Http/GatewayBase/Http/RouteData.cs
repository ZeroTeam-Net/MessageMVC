using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Agebull.Common.Logging;

using Agebull.EntityModel.Common;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn,ItemNullValueHandling = NullValueHandling.Ignore)]
    [DataContract]
    public class RouteData : IMessageItem
    {

        #region IMessageItem

        /// <summary>
        /// 唯一标识，UUID
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ID { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 生产时间戳,UNIX时间戳,自1970起秒数
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Timestamp { get; set; } = DateTime.Now.ToTimestamp();

        string IMessageItem.Topic { get => ApiHost; set => ApiHost = value; }
        string IMessageItem.Title { get => ApiName; set => ApiName = value; }
        string IMessageItem.Content { get => HttpContent; set => HttpContent = value; }

        /// <summary>
        /// 其他带外内容
        /// </summary>
        string IMessageItem.Tag { get; set; }

        /// <summary>
        /// 上下文内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        string IMessageItem.Context
        {
            get => GlobalContextJson ?? JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy);
            set => GlobalContextJson = value;
        }

        /// <summary>
        ///     上下文的JSON内容(透传)
        /// </summary>
        public string GlobalContextJson;

        #endregion

        #region Request

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        ///     请求地址
        /// </summary>
        [JsonProperty("uri", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Uri Uri { get; private set; }
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
        ///     Http Header中的Authorization信息
        /// </summary>
        [JsonProperty("token", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Token { get; set; }

        /// <summary>
        ///     HTTP method
        /// </summary>
        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string HttpMethod { get; private set; }

        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonProperty("Content", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string HttpContent { get; private set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        [JsonProperty("headers", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, List<string>> Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// HTTP的UserAgent
        /// </summary>
        [JsonProperty("userAgent", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string UserAgent { get; set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember]
        [JsonProperty("arguments", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, string> Arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 取参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => Arguments.TryGetValue(key, out var vl) ? vl : null;

        #endregion

        #region Response

        /// <summary>
        ///     缓存键
        /// </summary>
        public string CacheKey;

        /// <summary>
        ///     返回二进制值
        /// </summary>
        public Dictionary<string, byte[]> Files;

        /// <summary>
        ///     执行HTTP重写向吗
        /// </summary>
        [JsonProperty("redirect", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool Redirect;

        /// <summary>
        ///     返回文本值
        /// </summary>
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Result { get; set; }

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
        [DataMember]
        [JsonProperty("succeed", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)] 
        public bool IsSucceed => State == MessageState.Success;

        /// <summary>
        ///     开始时间
        /// </summary>
        [DataMember]
        [JsonProperty("start", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)] 
        public DateTime Start { get; set; } = DateTime.Now;

        /// <summary>
        ///     结束时间
        /// </summary>
        [DataMember]
        [JsonProperty("end", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime End { get; set; }

        /// <summary>
        /// 刷新操作
        /// </summary>
        public void Flush()
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
        ///     调用检查
        /// </summary>
        /// <param name="context"></param>
        public Task<bool> CheckRequest(HttpContext context)
        {
            HttpContext = context;
            var request = context.Request;
            Uri = request.GetUri();
            if (!CheckApiRoute())
                return Task.FromResult(false);

            HttpMethod = request.Method.ToUpper();
            CheckHeaders(context, request);

            if (MessageRoute.Option.EnableGlobalContext)
            {
                GlobalContext.SetRequestContext(new RequestInfo
                {
                    RequestId = $"{Token ?? "*"}-{RandomOperate.Generate(6)}",
                    UserAgent = UserAgent,
                    Token = Token,
                    RequestType = RequestType.Http,
                    ArgumentType = ArgumentType.Json,
                    Ip = request.Headers["X-Forwarded-For"].FirstOrDefault() ?? request.Headers["X-Real-IP"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString(),
                    Port = request.Headers["X-Real-Port"].FirstOrDefault() ?? context.Connection.RemotePort.ToString(),
                });
            }
            return ReadArgument(context);
        }

        private void CheckHeaders(HttpContext context, HttpRequest request)
        {
            if (MessageRoute.Option.EnableAuthToken)
            {
                Token = request.Headers["AUTHORIZATION"].LastOrDefault()?
                .Trim()
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                ?? context.Request.Query["token"];
                if (string.IsNullOrWhiteSpace(Token) || Token.Equals("null") || Token.Equals("undefined") || Token.Equals("Bearer"))
                {
                    Token = null;
                }
            }
            if (MessageRoute.Option.EnableUserAgent)
                UserAgent = request.Headers["USER-AGENT"].LinkToString("|");
            if (MessageRoute.Option.EnableHttpHeader)
            {
                foreach (var head in request.Headers)
                {
                    var key = head.Key.ToUpper();
                    switch (key)
                    {
                        case "USER-AGENT":
                        case "AUTHORIZATION":
                            break;
                        default:
                            Headers.Add(key, head.Value.ToList());
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckApiRoute()
        {
            var words = Uri.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                //UserState = UserOperatorStateType.FormalError;
                //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Result = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
            var idx = 0;
            MessageRoute.Option.HostPaths?.TryGetValue(Uri.Host, out idx);
            if (words.Length <= idx + 1)
            {
                //UserState = UserOperatorStateType.FormalError;
                //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Result = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
            ApiHost = words[idx];
            ApiName = string.Join('/', words.Skip(idx + 1));
            return true;
        }

        private async Task<bool> ReadArgument(HttpContext context)
        {
            var request = context.Request;
            try
            {
                if (request.QueryString.HasValue)
                {
                    foreach (var key in request.Query.Keys)
                        Arguments.TryAdd(key, request.Query[key]);
                }
                if (request.HasFormContentType)
                {
                    foreach (var key in request.Form.Keys)
                        Arguments.TryAdd(key, request.Form[key]);
                    if (!await ReadFiles(request))
                        return false;
                }

                if (request.ContentLength == null)
                    return true;
                using (var texter = new StreamReader(request.Body))
                {
                    HttpContent = await texter.ReadToEndAsync();
                    if (string.IsNullOrEmpty(HttpContent))
                        HttpContent = null;
                    texter.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "读取远程参数");
                //UserState = UserOperatorStateType.FormalError;
                //ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Result = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
        }

        private async Task<bool> ReadFiles(HttpRequest request)
        {
            if (!MessageRoute.Option.EnableFormFile)
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
                    Result = ApiResultIoc.ArgumentErrorJson;
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