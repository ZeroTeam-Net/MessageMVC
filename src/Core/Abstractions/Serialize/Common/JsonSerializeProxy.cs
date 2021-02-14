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

        static readonly JsonSerializerOptions aOptions = Options(false);
        static readonly JsonSerializerOptions bOptions = Options(true);
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

        /// <summary>
        /// 反序列化
        /// </summary>
        public T ToObject<T>(byte[] json)
        {
            if (json == null || json.Length == 0)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(json, Options(false));
        }

        /// <summary>
        /// 序列化妻字节
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>Utf8字节</returns>
        public byte[] ToBytes<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return default;
            }
            return JsonSerializer.SerializeToUtf8Bytes<T>(obj, Options(false));
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
                : JsonSerializer.Serialize(obj, indented ? bOptions : aOptions);
        }
    }
}