using System;
using System.Text.Json;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public class JsonSerializeProxy : IJsonSerializeProxy
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public object Serialize(object obj)
        {
            return obj == null
                ? null
                : JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="soruce">源内容(一般都是文本)</param>
        /// <param name="type">类型</param>
        /// <returns>结果对象，可能因为格式不良好而产生异常</returns>
        public object Deserialize(object soruce, Type type)
        {
            if (soruce is string json && !string.IsNullOrWhiteSpace(json))
                switch (json[0])
                {
                    case '{':
                    case '[':
                        return JsonSerializer.Deserialize(json, type);
                }
            return default;
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
                    return JsonSerializer.Deserialize<T>(json);
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
                    return JsonSerializer.Deserialize(json, type);
            }
            return default;
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return obj == null
                ? null
                : indented
                    ? JsonSerializer.Serialize(obj, new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                        WriteIndented = true
                    })
                    : JsonSerializer.Serialize(obj);
        }
    }
}