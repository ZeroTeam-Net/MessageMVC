using System;

namespace Agebull.Common.Configuration
{
    /// <summary>
    /// 表示来自配置文件构造的选项对象
    /// </summary>
    public class FromConfigAttribute : Attribute
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public FromConfigAttribute(string name)
        {
            Name = name;
        }

    }
}