using System;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// 表示一个序列化代理
    /// </summary>
    public interface ISerializeProxy
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        bool TryDeserialize(string soruce, Type type, out object dest)
        {
            if (soruce == null)
            {
                dest = default;
                return false;
            }
            try
            {
                dest = ToObject(soruce, type);
                return !Equals(dest, default);
            }
            catch
            {
                dest = default;
                return false;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        bool TryDeserialize<T>(string soruce, out T dest)
        {
            if (soruce == null)
            {
                dest = default;
                return false;
            }
            try
            {
                dest = ToObject<T>(soruce);
                return !Equals(dest, default);
            }
            catch
            {
                dest = default;
                return false;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        T ToObject<T>(string str);


        /// <summary>
        /// 反序列化
        /// </summary>
        object ToObject(string str, Type type);


        /// <summary>
        /// 序列化
        /// </summary>
        string ToString(object obj, bool indented = false);
    }
}