using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 序列化类型
    /// </summary>
    public enum SerializeType
    {
        /// <summary>
        /// 不需要
        /// </summary>
        None,
        /// <summary>
        /// JSON
        /// </summary>
        Json,
        /// <summary>
        /// Newtonsoft的JSON
        /// </summary>
        NewtonJson,
        /// <summary>
        /// XML
        /// </summary>
        Xml,
        /// <summary>
        /// gRPC的BSON
        /// </summary>
        Bson,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }

    /// <summary>
    /// 序列化类型
    /// </summary>
    public enum ArgumentScope
    {
        /// <summary>
        /// 内容
        /// </summary>
        Content,
        /// <summary>
        /// HTTP的URL的参数
        /// </summary>
        HttpArgument,
        /// <summary>
        /// HTTP的Form
        /// </summary>
        HttpForm
    }

    /// <summary>
    /// 表示自自定义序列化
    /// </summary>
    public class ArgumentScopeAttribute : Attribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public ArgumentScope Scope { get; }

        /// <summary>
        /// 构造
        /// </summary>
        public ArgumentScopeAttribute(ArgumentScope scope)
        {
            Scope = scope;
        }
    }
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