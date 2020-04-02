﻿namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息状态
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// 未消费
        /// </summary>
        None = 0,

        /// <summary>
        /// 取消处理
        /// </summary>
        Cancel = 1,

        /// <summary>
        /// 格式错误
        /// </summary>
        FormalError = 2,

        /// <summary>
        /// 网络传输错误
        /// </summary>
        NetError = 0x20,

        /// <summary>
        /// 已接受
        /// </summary>
        Accept = 0x30,

        /// <summary>
        /// 处理成功
        /// </summary>
        Success = 0x40,

        /// <summary>
        /// 无处理方法
        /// </summary>
        NoSupper = 0x41,

        /// <summary>
        /// 处理失败
        /// </summary>
        Failed = 0x42,

        /// <summary>
        /// 处理异常
        /// </summary>
        Exception = 0x43

    }
}
