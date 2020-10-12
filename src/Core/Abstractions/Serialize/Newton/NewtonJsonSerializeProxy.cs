using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public class NewtonJsonSerializeProxy : IJsonSerializeProxy
    {
        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new JsonNumberConverter(),
                new JsonEnumConverter()
            },
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

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
                    return JsonConvert.DeserializeObject<T>(json, Settings);
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
                    return JsonConvert.DeserializeObject(json, type, Settings);
            }
            return default;
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return obj == null
                ? null
                : indented
                    ? JsonConvert.SerializeObject(obj, Formatting.Indented, Settings)
                    : JsonConvert.SerializeObject(obj, Settings);
        }
    }
}