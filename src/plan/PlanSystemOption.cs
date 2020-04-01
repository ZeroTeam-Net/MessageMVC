namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 任务计划系统配置
    /// </summary>
    public class PlanSystemOption
    {
        /// <summary>
        /// 系统配置,后期注入
        /// </summary>
        public static PlanSystemOption Option { get; set; }

        /// <summary>
        /// 执行超时(MS),默认30000(30秒)
        /// </summary>
        public int ExecTimeout { get; set; } = 30000;

        /// <summary>
        /// 关闭后的数据过期时间(MS),无效时间立即删除
        /// </summary>
        public int CloseTimeout { get; set; }

        /// <summary>
        /// 轮询空转时间(MS),默认300
        /// </summary>
        public int LoopIdleTime { get; set; } = 300;

        /// <summary>
        /// 最大并行的任务数,默认128
        /// </summary>
        /// <remarks>
        /// 当已开始未结束的任务达到这个数后,系统将进入等待状态
        /// </remarks>
        public int MaxRunTask { get; set; } = 128;


        /// <summary>
        /// 是否检查执行结果(要求执行结果为ApiResult的Json文本)
        /// </summary>
        public bool CheckPlanResult { get; set; }


        /// <summary>
        /// 保存检查执行结果(可能激增Redis的内存使用)
        /// </summary>
        public bool SavePlanResult { get; set; }

        /// <summary>
        /// 因服务无法访问等原因执行异常的重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 重试延期时长(MS),默认3000,10000,600000
        /// </summary>
        public int[] RetryDelay { get; set; } = { 3000, 10000, 600000 };
    }

}