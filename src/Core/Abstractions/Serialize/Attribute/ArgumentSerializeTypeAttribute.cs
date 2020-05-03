using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 参数序列化
    /// </summary>
    public class ArgumentSerializeTypeAttribute : Attribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public SerializeType Type { get; }

        /// <summary>
        /// 构造
        /// </summary>
        public ArgumentSerializeTypeAttribute(SerializeType type)
        {
            Type = type;
        }
    }
}