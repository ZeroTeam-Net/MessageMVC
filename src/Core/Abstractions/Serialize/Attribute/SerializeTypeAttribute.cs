using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示自自定义序列化
    /// </summary>
    public class SerializeTypeAttribute : Attribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public SerializeType Type { get; }

        /// <summary>
        /// 构造
        /// </summary>
        public SerializeTypeAttribute(SerializeType type)
        {
            Type = type;
        }
    }
}