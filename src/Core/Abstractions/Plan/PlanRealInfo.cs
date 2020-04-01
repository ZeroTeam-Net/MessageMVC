using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    /// <summary>
    /// 计划实时信息
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PlanRealInfo
    {
        /// <summary>
        /// 错误次数
        /// </summary>
        public int error_repet;

        /// <summary>
        /// 返回次数
        /// </summary>
        public int real_repet;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int retry_repet;

        /// <summary>
        /// 跳过次数计数,
        /// 1 当no_skip=true时,空跳也会参与计数.
        /// 2 此计数在执行时发生,
        ///     2.1 skip_set &lt; 0 直接计算下一次执行时间,
        ///     2.2 在skip_set &gt; 0时,skip_set &lt; skip_num时直接计算下一次执行时间,否则正常执行
        /// </summary>
        public int skip_num;

        /// <summary>
        /// 跳过设置次数(1-n 跳过次数)
        /// </summary>
        public int skip_set;

        /// <summary>
        /// 最后一次执行状态
        /// </summary>
        public MessageState exec_state;

        /// <summary>
        /// 计划状态
        /// </summary>
        public Plan_message_state plan_state;

        /// <summary>
        /// 加入时间
        /// </summary>
        public long add_time;

        /// <summary>
        /// 计划时间
        /// </summary>
        public long plan_time;

        /// <summary>
        /// 执行次数
        /// </summary>
        public int exec_repet;

        /// <summary>
        /// 开始时间
        /// </summary>
        public long exec_start_time;

        /// <summary>
        /// 完成时间
        /// </summary>
        public long exec_end_time;
    }
}