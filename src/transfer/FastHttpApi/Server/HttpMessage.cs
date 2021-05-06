using BeetleX.FastHttpApi.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZeroTeam.MessageMVC.Messages;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HttpMessage : MessageItem, IInlineMessage, IDataContext
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
        [JsonIgnore]
        public Dictionary<string, byte[]> BinaryDictionary { get; set; }

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
        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonIgnore]
        public string RequestId { get; internal set; }

        /// <summary>
        ///     Http上下文
        /// </summary>
        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonIgnore]
        public HttpRequest Request { get; internal set; }

        /// <summary>
        ///     Http上下文
        /// </summary>
        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonIgnore]
        public IHttpContext HttpContext { get; internal set; }

        /// <summary>
        ///     请求地址
        /// </summary>
        [JsonIgnore]
        public string Url { get; internal set; }

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
        public JToken ContentObject { get; set; }

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
        /// 取基本参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        public string GetStringArgument(string name)
        {
            var data = ContentObject[name];
            if (data != null)
                return data.Value<string>();
            if (ExtensionDictionary.TryGetValue(name, out var ar))
                return ar;
            return null;
        }

        /// <summary>
        /// 取二进制参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        public byte[] GetBinaryArgument(string name)
        {
            if (BinaryDictionary == null)
                return null;
            BinaryDictionary.TryGetValue(name, out var str);
            return str;
        }
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
                    return ExtensionDictionary.Count > 0 ? serialize.ToString(ExtensionDictionary) : null;
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
            var data = ContentObject[name];
            if (data != null)
                return data.Value<string>();
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
        string IInlineMessage.Look()
        {
            var code = new StringBuilder();
            code.AppendLine($"ID:{ID}");
            code.AppendLine($"URL:{Request.Url}");
            code.AppendLine($"Trace:{JsonConvert.SerializeObject(TraceInfo, Formatting.Indented)}");

            if (ExtensionDictionary != null && ExtensionDictionary.Count > 0)
                code.AppendLine($"Dictionary:{JsonConvert.SerializeObject(ExtensionDictionary, Formatting.Indented)}");
            if (HttpContent != null)
                code.AppendLine($"Content:{HttpContent}");

            return code.ToString();
        }

        #endregion

        #region IDataContext

        public string this[string name]
        {
            get
            {
                TryGetString(name, out string result);
                return result;
            }
        }

        public void Clear()
        {
            ExtensionDictionary.Clear();
        }

        public void SetValue(string name, object value)
        {
            ExtensionDictionary[name] = value?.ToString();
        }

        protected bool GetProperty(string name, out object value)
        {
            if (ExtensionDictionary.TryGetValue(name, out var text))
            {
                value = text;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetBoolean(string name, out bool value)
        {
            value = false;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Boolean)
                    {
                        string str = token.ToObject<string>();
                        return bool.TryParse((string)str, out value);
                    }
                    value = token.ToObject<bool>();
                    return true;
                }
                else
                    return Boolean.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetDateTime(string name, out DateTime value)
        {
            value = DateTime.Now;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Date)
                    {
                        string str = token.ToObject<string>();
                        return DateTime.TryParse((string)str, out value);
                    }
                    value = token.ToObject<DateTime>();
                    return true;
                }
                else
                    return DateTime.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetDecimal(string name, out decimal value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Float)
                    {
                        string str = token.ToObject<string>();
                        return decimal.TryParse((string)str, out value);
                    }
                    value = token.ToObject<decimal>();
                    return true;
                }
                else
                    return decimal.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetDouble(string name, out double value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Float)
                    {
                        string str = token.ToObject<string>();
                        return double.TryParse((string)str, out value);
                    }
                    value = token.ToObject<double>();
                    return true;
                }
                else
                    return double.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetFloat(string name, out float value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Float)
                    {
                        string str = token.ToObject<string>();
                        return float.TryParse((string)str, out value);
                    }
                    value = token.ToObject<float>();
                    return true;
                }
                else
                    return float.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetInt(string name, out int value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return int.TryParse((string)str, out value);
                    }
                    value = token.ToObject<int>();
                    return true;
                }
                else
                    return int.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetLong(string name, out long value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return long.TryParse((string)str, out value);
                    }
                    value = token.ToObject<long>();
                    return true;
                }
                else
                    return long.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetShort(string name, out short value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return short.TryParse((string)str, out value);
                    }
                    value = token.ToObject<short>();
                    return true;
                }
                else
                    return short.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetUInt(string name, out uint value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return uint.TryParse((string)str, out value);
                    }
                    value = token.ToObject<uint>();
                    return true;
                }
                else
                    return uint.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetULong(string name, out ulong value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return ulong.TryParse((string)str, out value);
                    }
                    value = token.ToObject<ulong>();
                    return true;
                }
                else
                    return ulong.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetUShort(string name, out ushort value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return ushort.TryParse((string)str, out value);
                    }
                    value = token.ToObject<ushort>();
                    return true;
                }
                else
                    return ushort.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetByte(string name, out byte value)
        {
            value = 0;
            if (GetProperty(name, out object data))
            {
                if (data is JProperty token)
                {
                    if (token.Type != JTokenType.Integer)
                    {
                        string str = token.ToObject<string>();
                        return byte.TryParse((string)str, out value);
                    }
                    value = token.ToObject<byte>();
                    return true;
                }
                else
                    return byte.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetChar(string name, out char value)
        {
            value = (char)0;
            if (GetProperty(name, out object data))
            {
                if (data is JToken token)
                    value = token.ToObject<char>();
                else
                    return char.TryParse((string)data, out value);
            }
            return false;
        }

        public bool TryGetString(string name, out string value)
        {
            value = null;
            if (GetProperty(name, out object data))
            {
                if (data is JToken token)
                    value = token.ToObject<string>();
                else
                    value = (string)data;
                return true;
            }
            return false;
        }

        public object GetObject(string name, Type type)
        {
            if (GetProperty(name, out object value))
            {
                if (value is JToken token)
                {
                    JProperty jProperty = token as JProperty;
                    if (jProperty != null)
                        return jProperty.Value.ToObject(type);
                    else
                        return token.ToObject(type);
                }
                else
                {
                    if (value is string @string)
                    {
                        if (type.IsEnum)
                            return Enum.Parse(type, @string);
                        else
                            return JsonConvert.DeserializeObject(@string, type);
                    }
                    return value;
                }
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var item in ExtensionDictionary)
                sb.AppendFormat("{0}={1}\r\n", item.Key, item.Value);
            return sb.ToString();
        }

        public IDictionary<string, object> Copy()
        {
            Dictionary<string, object> result = new();
            foreach (var item in ExtensionDictionary)
                result[item.Key] = item.Value;
            return result;
        }
        #endregion

    }
}
