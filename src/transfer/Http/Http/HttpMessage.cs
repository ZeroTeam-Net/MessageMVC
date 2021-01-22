using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
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
        /// <summary>
        /// 消息类型
        /// </summary>
        string IInlineMessage.MessageType => "HttpMessage";

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
        public Dictionary<string, string> ExtensionDictionary { get; set; }

        /// <summary>
        /// 二进制字典参数
        /// </summary>
        public Dictionary<string, byte[]> BinaryDictionary { get; set; }

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
        ///     请求地址
        /// </summary>
        [JsonIgnore]
        public string Uri { get; internal set; }

        /// <summary>
        ///     HTTP method
        /// </summary>
        [JsonIgnore]
        public string HttpMethod { get; internal set; }

        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonIgnore]
        public string HttpContent { get; set; }

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
                case ArgumentScope.Dictionary:
                    return ExtensionDictionary.Count > 0? serialize.ToString(ExtensionDictionary) : null;
                default:
                    return HttpContent;
            }
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
        public string GetScopeArgument(string name, ArgumentScope scope = ArgumentScope.Dictionary)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            switch (scope)
            {
                case ArgumentScope.Dictionary:
                    if (ExtensionDictionary.TryGetValue(name, out var ha))
                        return ha;
                    return null;
            }
            ContentObject ??= string.IsNullOrWhiteSpace(HttpContent)
                    ? new JObject()
                    : (JObject)JsonConvert.DeserializeObject(HttpContent);

            if (ContentObject.TryGetValue(name, out var vl))
                return vl?.ToString();
            if (ExtensionDictionary.TryGetValue(name, out var ar))
                return ar;
            return null;
        }

        #endregion

        #region 状态
        /*
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
                DependencyRun.Logger.Exception(e);
                State = MessageState.FormalError;
            }
            return Task.CompletedTask;
        }
        */
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
            code.AppendLine($"URL:{HttpContext.Request.Path}");
            code.AppendLine($"Trace:{JsonConvert.SerializeObject(Trace, Formatting.Indented)}");

            if (ExtensionDictionary != null && ExtensionDictionary.Count > 0)
                code.AppendLine($"Dictionary:{JsonConvert.SerializeObject(ExtensionDictionary, Formatting.Indented)}");
            if (HttpContent != null)
                code.AppendLine($"Content:{HttpContent}");

            return code.ToString();
        }

        #endregion
    }
}