using System;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 跟踪内容
    /// </summary>
    [Flags]
    public enum TraceInfoType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0x0,

        /// <summary>
        /// 应用信息
        /// </summary>
        App = 0x1,

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
        /// 应用信息
        /// </summary>
        All = 0xFFFF
    }
}