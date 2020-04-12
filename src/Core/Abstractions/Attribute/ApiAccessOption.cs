using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API访问配置
    /// </summary>
    [Flags]
    public enum ApiAccessOption
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
        ///     内部访问(内部Poster)
        /// </summary>
        Internal = 0x2,

        /// <summary>
        ///     匿名访问
        /// </summary>
        Anymouse = 0x4,

        /// <summary>
        ///     需要身份验证
        /// </summary>
        Authority = 0x8,

        /// <summary>
        ///     参数可以为null
        /// </summary>
        ArgumentCanNil = 0x10,

        /// <summary>
        ///     参数仅为定义,内部进行通过字典读取
        /// </summary>
        DictionaryArgument = 0x20,

        /// <summary>
        /// 开放访问,即公开且匿名访问
        /// </summary>
        OpenAccess = Public | Anymouse,

        /// <summary>
        /// 用户访问,即公开且需要身份验证
        /// </summary>
        UserAccess = Public | Authority,

        /// <summary>
        ///     参数仅为定义,内部进行通过字典读取
        /// </summary>
        ArgumentIsDefault = DictionaryArgument
    }
}