namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划类型
    /// </summary>
    public enum plan_date_type
    {
        /// <summary>
        /// 无计划，立即发送
        /// </summary>
        none,
        /// <summary>
        /// 在指定的时间发送
        /// </summary>
        time,
        /// <summary>
        /// 秒间隔后发送
        /// </summary>
        second,
        /// <summary>
        /// 分钟间隔后发送
        /// </summary>
        minute,
        /// <summary>
        /// 小时间隔后发送
        /// </summary>
        hour,
        /// <summary>
        /// 日间隔后发送
        /// </summary>
        day,
        /// <summary>
        /// 每周几
        /// </summary>
        week,
        /// <summary>
        /// 每月几号
        /// </summary>
        month
    }
}