using System;

namespace ZeroTeam.MessageMVC.Messages
{
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
}