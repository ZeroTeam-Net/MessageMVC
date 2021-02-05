using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表明消息的数据状态
    /// </summary>
    [Flags]
    public enum MessageDataState
    {
        /// <summary>
        /// 未知
        /// </summary>
        None = 0,

        /// <summary>
        /// 参数已在线
        /// </summary>
        ArgumentInline = 0x1,

        /// <summary>
        /// 参数已离线
        /// </summary>
        ArgumentOffline = 0x2,

        /// <summary>
        /// 返回值已在线
        /// </summary>
        ResultInline = 0x4,

        /// <summary>
        /// 返回值已离线
        /// </summary>
        ResultOffline = 0x8,

        /// <summary>
        /// 扩展参数已在线
        /// </summary>
        ExtensionInline = 0x10,

        /// <summary>
        /// 扩展参数已离线
        /// </summary>
        ExtensionOffline = 0x20,

    }
}
