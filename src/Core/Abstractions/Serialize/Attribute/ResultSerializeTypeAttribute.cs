using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 返回值序列化
    /// </summary>
    public class ResultSerializeTypeAttribute : Attribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public SerializeType Type { get; }

        /// <summary>
        /// 构造
        /// </summary>
        public ResultSerializeTypeAttribute(SerializeType type)
        {
            Type = type;
        }
    }
}