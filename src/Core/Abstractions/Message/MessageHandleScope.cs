using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息的处理范围
    /// </summary>
    [Flags]
    public enum MessageHandleScope : byte
    {
        /// <summary>
        /// 什么也不处理
        /// </summary>
        None = 0x0,
        /// <summary>
        /// 处理消息
        /// </summary>
        Handle = 0x1,
        /// <summary>
        /// 处理全局异常
        /// </summary>
        Exception = 0x2,
        /// <summary>
        /// 处理最终结果
        /// </summary>
        End = 0x4
    }
}