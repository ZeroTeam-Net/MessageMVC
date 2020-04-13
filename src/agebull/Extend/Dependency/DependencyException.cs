using System;

namespace Agebull.Common.Ioc
{
    /// <summary>
    ///    依赖对象异常
    /// </summary>
    public class DependencyException : Exception
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public DependencyException(Type type, string message) : base($"{message},依赖类型{type.GetFullTypeName()}")
        {

        }
    }
}