using System;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 跟踪内容
    /// </summary>
    [Flags]
    public enum MessageTraceType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0x0,

        /// <summary>
        /// 请求信息
        /// </summary>
        Request = 0x1,

        /// <summary>
        /// 头信息
        /// </summary>
        Headers = 0x2,

        /// <summary>
        /// 令牌
        /// </summary>
        Token = 0x4,

        /// <summary>
        /// 链路
        /// </summary>
        LinkTrace = 0x8,

        /// <summary>
        /// 用户
        /// </summary>
        User = 0x10,

        /// <summary>
        /// 上下文
        /// </summary>
        Context = 0x20,

        /// <summary>
        /// 简单信息
        /// </summary>
        Simple = User | LinkTrace,

        /// <summary>
        /// 应用信息
        /// </summary>
        All = Request | Headers | Token | LinkTrace | User | Context,


        /// <summary>
        /// 独立，不使用全局配置
        /// </summary>
        Isolate = 0x100,

        /// <summary>
        /// 未定义，使用全局配置
        /// </summary>
        Undefined = 0x200

    }
}