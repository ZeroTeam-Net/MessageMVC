using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    /// <summary>
    /// 计划实时信息
    /// </summary>
    public class PlanRealInfo
    {
        /// <summary>
        /// 执行次数
        /// </summary>
        public int ExecNum { get; set; }

        /// <summary>
        /// 返回次数
        /// </summary>
        public int SuccessNum { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        public int ErrorNum { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryNum { get; set; }

        /// <summary>
        /// 跳过次数计数,
        /// 1 当no_skip=true时,空跳也会参与计数.
        /// 2 此计数在执行时发生,
        ///     2.1 skip_set &lt; 0 直接计算下一次执行时间,
        ///     2.2 在skip_set &gt; 0时,skip_set &lt; skip_num时直接计算下一次执行时间,否则正常执行
        /// </summary>
        public int SkipNum { get; set; }

        /// <summary>
        /// 最后一次执行状态
        /// </summary>
        public MessageState ExecState { get; set; }

        /// <summary>
        /// 计划状态
        /// </summary>
        public PlanMessageState PlanState { get; set; }

        /// <summary>
        /// 计划时间
        /// </summary>
        public long PlanTime { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public long ExecStartTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public long ExecEndTime { get; set; }
    }
}