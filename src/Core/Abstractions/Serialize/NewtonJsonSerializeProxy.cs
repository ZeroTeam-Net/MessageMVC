using System;
using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Messages
{


    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public class NewtonJsonSerializeProxy : IJsonSerializeProxy
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
                : JsonConvert.SerializeObject(obj, new JsonNumberConverter(), new JsonEnumConverter());
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
                        return JsonConvert.DeserializeObject(json, type, new JsonNumberConverter(), new JsonEnumConverter());
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
                    return JsonConvert.DeserializeObject<T>(json, new JsonNumberConverter(), new JsonEnumConverter());
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
                    return JsonConvert.DeserializeObject(json, type, new JsonNumberConverter(), new JsonEnumConverter());
            }
            return default;
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return obj == null
                ? null
                : indented
                    ? JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonNumberConverter(), new JsonEnumConverter())
                    : JsonConvert.SerializeObject(obj, new JsonNumberConverter(), new JsonEnumConverter());
        }
    }
}