namespace ZeroTeam.MessageMVC.Messages
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
        /// 已接受
        /// </summary>
        Accept = 0x20,

        /// <summary>
        /// 消息接收错误
        /// </summary>
        NetworkError = 0x30,

        /// <summary>
        /// 处理成功
        /// </summary>
        Success = 0x40,

        /// <summary>
        /// 不支持处理
        /// </summary>
        NonSupport = 0x41,

        /// <summary>
        /// 处理失败
        /// </summary>
        Failed = 0x42,

        /// <summary>
        /// 处理错误
        /// </summary>
        Error = 0x43

    }
}
