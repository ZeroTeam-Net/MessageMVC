using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public class JsonSerializeProxy : IJsonSerializeProxy
    {
        static JsonSerializerOptions Options(bool indented)
        {
            var option = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = indented,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            option.Converters.Add(new JsonStringEnumConverter());
            return option;
        }

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
                    return JsonSerializer.Deserialize<T>(json, Options(false));
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
                    return JsonSerializer.Deserialize(json, type, Options(false));
            }
            return default;
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return obj == null
                ? null
                : JsonSerializer.Serialize(obj, Options(indented));
        }
    }
}