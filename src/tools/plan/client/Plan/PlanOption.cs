namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划配置
    /// </summary>
    public class PlanOption
    {
        /// <summary>
        /// 数据标识
        /// </summary>
        public long DataId { get; set; }

        /// <summary>
        /// 消息标识
        /// </summary>
        public string PlanId { get; set; }

        /// <summary>
        /// 计划说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 计划类型
        /// </summary>
        public PlanTimeType PlanType { get; set; }

        /// <summary>
        /// 类型值
        /// </summary>
        /// <remarks>
        /// none time 无效
        /// second minute hour day : 延时处理的 指定延时数量(单位为对应的plan_date_type)
        /// week : 周日到周六(0-6),值无效系统自动放弃(无提示)
        /// month: 正数为指定号数(如当月不存在,则使用当月最后一天) 零或负数为月未倒推(0为最后一天,负数为减去的数字,减的结果为0或负数的,则为当前第一天)
        /// </remarks>
        public short PlanValue { get; set; }

        /// <summary>
        /// 重试次数(1-n次数,-1重试,0不重试)
        /// </summary>
        public int RetrySet { get; set; }

        /// <summary>
        /// 跳过设置次数(1-n 跳过次数)
        /// </summary>
        public int SkipSet { get; set; }

        /// <summary>
        /// 重复次数,0不重复 >0重复次数,-1永久重复
        /// </summary>
        /// <remarks>
        /// none time 无效
        /// second minute hour day : 延时处理的,如指定时间太小,
        /// no_keep=true: 多次计算时间如果小于当前的,将密集执行
        /// no_keep=false: 每次空跳(系统不执行操作)也算一次,如时间足够小,可能全是空跳而未能执行一次,除非指定不允许空跳()
        /// </remarks>
        public int PlanRepet { get; set; }

        /// <summary>
        /// 跳过无效时间
        /// </summary>
        /// <remarks>
        /// 执行时并不检查时间是否已过去
        /// </remarks>
        public bool QueuePassBy { get; set; }

        /// <summary>
        /// 加入时间
        /// </summary>
        public long AddTime { get; set; }

        /// <summary>
        /// 计划时间
        /// </summary>
        /// <remarks>
        /// 使用UNIX时间(1970年1月1日0时0分0秒起的总毫秒数)
        /// plan_type :
        ///  none 无效,系统自动分配当前时间以便下一次立即执行,plan_repet无效
        ///  time 指定时间,如时间过期,系统自动放弃,plan_repet无效
        ///  second minute hour day : 延时处理的,如指定,则以此时间为基准,否则以系统接收时间为基准(可能产生误差)
        ///  week month:指定日期的,只使用此时间的天内部分(即此时间可以明确指定引发的时间),如不指定,则时间为0:0:0
        /// </remarks>
        public long PlanTime { get; set; }

        /// <summary>
        ///     异步调用
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        ///     异步或无结果时检查结果时长
        /// </summary>
        public int CheckResultTime { get; set; }
    }
}