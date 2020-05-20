using Newtonsoft.Json;
using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public class NewtonJsonSerializeProxy : IJsonSerializeProxy
    {
        static readonly JsonNumberConverter numberConverter = new JsonNumberConverter();

        static readonly JsonEnumConverter enumConverter = new JsonEnumConverter();

        /// <summary>
        /// 反序列化
        /// </summary>
        public T ToObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }
            switch (json[0])
            {
                case '{':
                case '[':
                    return JsonConvert.DeserializeObject<T>(json, numberConverter, enumConverter);
            }
            return default;
        }
        ///<inheritdoc/>
        public object ToObject(string json, Type type)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }
            switch (json[0])
            {
                case '{':
                case '[':
                    return JsonConvert.DeserializeObject(json, type, numberConverter, enumConverter);
            }
            return default;
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return obj == null
                ? null
                : indented
                    ? JsonConvert.SerializeObject(obj, Formatting.Indented, numberConverter, enumConverter)
                    : JsonConvert.SerializeObject(obj, numberConverter, enumConverter);
        }
    }
}