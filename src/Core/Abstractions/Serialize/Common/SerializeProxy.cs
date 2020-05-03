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