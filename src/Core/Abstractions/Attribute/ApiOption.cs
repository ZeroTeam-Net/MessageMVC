using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API访问配置
    /// </summary>
    [Flags]
    public enum ApiOption
    {
        /// <summary>
        ///     不可访问
        /// </summary>
        None,

        /// <summary>
        ///     公开访问(网关入口)
        /// </summary>
        Public = 0x1,

        /// <summary>
        ///     匿名访问
        /// </summary>
        Anymouse = 0x2,

        /// <summary>
        ///     只读操作
        /// </summary>
        Readonly = 0x4,

        /// <summary>
        ///     参数可以为null
        /// </summary>
        ArgumentCanNil = 0x10,

        /// <summary>
        ///     参数仅为定义,内部进行通过字典读取
        /// </summary>
        DictionaryArgument = 0x20,

        /// <summary>
        ///     内容自定义解析
        /// </summary>
        CustomContent = 0x40
    }
}