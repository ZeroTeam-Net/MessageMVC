using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 任务计划系统配置
    /// </summary>
    internal class PlanSystemOption
    {
        /// <summary>
        /// 空转时间
        /// </summary>
        public const int idleTime = 10000;

        /// <summary>
        /// 恢复等待时间
        /// </summary>
        public const int waitTime = 30000;

        /// <summary>
        /// 执行超时(MS),默认30000(30秒)
        /// </summary>
        public int ExecTimeout { get; set; }

        /// <summary>
        /// 关闭后的数据过期时间(S),无效时间立即删除
        /// </summary>
        public int CloseTimeout { get; set; }

        /// <summary>
        /// 轮询空转时间(MS),默认300
        /// </summary>
        public int LoopIdleTime { get; set; }

        /// <summary>
        /// 最大并行的任务数,默认128
        /// </summary>
        /// <remarks>
        /// 当已开始未结束的任务达到这个数后,系统将进入等待状态
        /// </remarks>
        public int MaxRunTask { get; set; }


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
        public int RetryCount { get; set; }

        /// <summary>
        /// 重试延期时长(MS),默认3000,10000,600000
        /// </summary>
        public int[] RetryDelay { get; set; }


        #region 配置同步


        /// <summary>
        /// 初始化
        /// </summary>
        static void Load()
        {
            var option = ConfigurationManager.Get<PlanSystemOption>("MessageMVC:PlanTask");
            if (option == null)
            {
                return;
            }
            Instance.SavePlanResult = option.SavePlanResult;
            Instance.CheckPlanResult = option.CheckPlanResult;
            if (option.ExecTimeout >= 0)
                Instance.CloseTimeout = option.CloseTimeout;
            if (option.ExecTimeout >= 0)
                Instance.ExecTimeout = option.ExecTimeout;
            if (option.LoopIdleTime >= 0)
                Instance.LoopIdleTime = option.LoopIdleTime;
            if (option.MaxRunTask >= 0)
                Instance.MaxRunTask = option.MaxRunTask;
            if (option.RetryCount >= 0)
                Instance.RetryCount = option.RetryCount;
            if (option.RetryDelay != null && option.RetryDelay.Length > 0)
                Instance.RetryDelay = option.RetryDelay;
        }
        /// <summary>
        /// 计划的系统配置
        /// </summary>
        public readonly static PlanSystemOption Instance = new PlanSystemOption
        {
            ExecTimeout = 30000,
            LoopIdleTime = 300,
            MaxRunTask = 128,
            RetryCount = 3,
            RetryDelay = new[] { 3000, 10000, 600000 }
        };


        /// <summary>
        /// 初始化
        /// </summary>
        static PlanSystemOption()
        {
            ConfigurationManager.RegistOnChange("MessageMVC:PlanTask", Load, true);
        }
        #endregion

    }
}