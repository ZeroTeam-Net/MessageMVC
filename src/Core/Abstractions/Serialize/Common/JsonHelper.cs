using System;
using System.Text;
using Agebull.Common;
using Agebull.Common.Ioc;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// Json序列化装饰器
    /// </summary>
    public static class JsonHelper
    {
        static readonly IJsonSerializeProxy porxy = DependencyHelper.GetService<IJsonSerializeProxy>() ?? NewtonJson;

        /// <summary>
        /// 使用NewtonsoftJson的序列化器
        /// </summary>
        public static readonly NewtonJsonSerializeProxy NewtonJson = new NewtonJsonSerializeProxy();

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="t"></param>
        /// <param name="indented">是否格式化</param>
        /// <returns></returns>
        public static string ToJson(this object t, bool indented = false)
        {
            return porxy.ToString(t, indented);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T t)
        {
            return porxy.ToString(t, false);
        }


        /// <summary>
        /// 反序列化
        /// </summary>
        public static T DeserializeObject<T>(string json)
        {
            return porxy.ToObject<T>(json);
        }


        /// <summary>
        /// 反序列化
        /// </summary>
        public static bool TryDeserializeObject<T>(string json, out T t)
        {
            try
            {
                t = porxy.ToObject<T>(json);
                return !Equals(t, default);
            }
            catch
            {
                t = default;
                return false;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static T TryDeserializeObject<T>(string json)
        {
            try
            {
                return porxy.ToObject<T>(json);
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
            return porxy.ToObject(json, type);
        }


        /// <summary>
        /// 转为UTF8字节
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>字节</returns>
        public static byte[] ToJsonBytes<T>(this T obj)
            where T : class
        {
            var json = SerializeObject(obj);
            return json == null ? ByteHelper.EmptyBytes : Encoding.UTF8.GetBytes(json);
        }

    }
}