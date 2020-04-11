using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 值类型序列化代理
    /// </summary>
    public class SerializeProxy : ISerializeProxy
    {
        /// <summary>
        /// 转换器
        /// </summary>
        public Func<object, object> Convert { get; set; }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <returns></returns>
        public object Serialize(object obj)
        {
            return obj?.ToString();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj">源内容(一般都是文本)</param>
        /// <param name="type">类型</param>
        /// <returns>结果对象，可能因为格式不良好而产生异常</returns>
        public object Deserialize(object obj, Type type)
        {
            return Convert == null ? obj : Convert(obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public T ToObject<T>(string str)
        {
            return (T)(Convert == null ? str : Convert(str));
        }

        ///<inheritdoc/>
        public object ToObject(string str, Type type)
        {
            return Convert == null ? null : Convert(str);
        }
        ///<inheritdoc/>
        public string ToString(object obj, bool indented)
        {
            return obj?.ToString();
        }
    }
}