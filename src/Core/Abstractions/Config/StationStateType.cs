namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 站点状态
    /// </summary>
    public enum StationStateType
    {
        /// <summary>
        /// 无，刚构造
        /// </summary>
        None = 0,

        /// <summary>
        /// 配置错误
        /// </summary>
        ConfigError = 1,

        /// <summary>
        /// 错误状态
        /// </summary>
        Failed = 2,

        /// <summary>
        /// 已初始化
        /// </summary>
        Initialized = 3,

        /// <summary>
        /// 正在运行
        /// </summary>
        Run = 6,


        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 9,

        /// <summary>
        /// 已被关停
        /// </summary>
        Stop = 12,

        /// <summary>
        /// 已被移除
        /// </summary>
        Remove = 13
    }
}