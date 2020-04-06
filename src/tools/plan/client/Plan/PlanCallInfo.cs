using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划调用信息
    /// </summary>
    public class PlanCallInfo
    {
        /// <summary>
        /// 计划配置
        /// </summary>
        public PlanOption Option { get; set; }

        /// <summary>
        /// 计划对象
        /// </summary>
        public MessageItem Message { get; set; }

    }
}