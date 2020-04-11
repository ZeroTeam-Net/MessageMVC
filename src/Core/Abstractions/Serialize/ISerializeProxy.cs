using System;

namespace ZeroTeam.MessageMVC.Messages
{

    /// <summary>
    /// 表示一个序列化代理
    /// </summary>
    public interface ISerializeProxy
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="soruce">对象</param>
        /// <returns></returns>
        object Serialize(object soruce);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="soruce">文本</param>
        /// <param name="type">类型</param>
        /// <returns>结果对象，可能因为格式不良好而产生异常</returns>
        object Deserialize(object soruce, Type type);

        /// <summary>
        /// 反序列化
        /// </summary>
        bool TryDeserialize(object soruce, Type type, out object dest)
        {
            if (soruce == null)
            {
                dest = default;
                return false;
            }
            try
            {
                dest = Deserialize(soruce, type);
                return true;
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