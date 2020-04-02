using System;
using System.Text;
using Newtonsoft.Json;

namespace Agebull.Common
{
    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 空字节
        /// </summary>
        public static readonly byte[] EmptyBytes =new byte[0];

        /// <summary>
        /// 转为UTF8字节
        /// </summary>
        /// <param name="str"></param>
        /// <returns>字节</returns>
        public static byte[] ToZeroBytes(this string str)
        {
            return str == null ? EmptyBytes : Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// 转为UTF8字节
        /// </summary>
        /// <param name="v"></param>
        /// <returns>字节</returns>
        public static byte[] ToZeroBytes<T>(this T v) where T : class
        {
            return v == null ? EmptyBytes : Encoding.UTF8.GetBytes(SerializeObject(v));
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T t)
        {
            return Equals(t, default)
                ? null
                : JsonConvert.SerializeObject(t, new JsonNumberConverter());
        }


        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJson(this object t)
        {
            return Equals(t, default)
                ? null
                : JsonConvert.SerializeObject(t, new JsonNumberConverter());
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T t)
        {
            return Equals(t, default)
                ? null
                : JsonConvert.SerializeObject(t, new JsonNumberConverter());
        }


        /// <summary>
        /// 反序列化
        /// </summary>
        public static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;
            switch (json[0])
            {
                case '{':
                case '[':
                    return JsonConvert.DeserializeObject<T>(json);
            }
            return default;
        }


        /// <summary>
        /// 反序列化
        /// </summary>
        public static T TryDeserializeObject<T>(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    return default;
                switch (json[0])
                {
                    case '{':
                    case '[':
                        return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch
            {
            }
            return default;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        public static object DeserializeObject(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
                return default;
            switch (json[0])
            {
                case '{':
                case '[':
                    return JsonConvert.DeserializeObject(json, type);
            }
            return default;
        }
    }
}