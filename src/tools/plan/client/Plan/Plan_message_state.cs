namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划状态
    /// </summary>
    public enum Plan_message_state
    {
        /// <summary>
        /// 无状态
        /// </summary>
        none,
        /// <summary>
        /// 排队
        /// </summary>
        queue,
        /// <summary>
        /// 正常执行
        /// </summary>
        execute,
        /// <summary>
        /// 重试执行
        /// </summary>
        retry,
        /// <summary>
        /// 跳过
        /// </summary>
        skip,
        /// <summary>
        /// 暂停
        /// </summary>
        pause,
        /// <summary>
        /// 错误关闭
        /// </summary>
        error,
        /// <summary>
        /// 正常关闭
        /// </summary>
        close,
        /// <summary>
        /// 删除
        /// </summary>
        remove
    }
}