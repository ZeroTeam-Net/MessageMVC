using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// Redis的配置项
    /// </summary>
    public class RedisOption
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <example>
        /// $"{Address}:{Port},password={PassWord},defaultDatabase={db},poolsize=50,ssl=false,writeBuffer=10240";
        /// </example>
        public string ConnectionString { get; set; }


        /// <summary>
        /// 异常守卫多久检查一次
        /// </summary>

        public int GuardCheckTime { get; set; }


        /// <summary>
        /// 消息处理过程锁定时长
        /// </summary>

        public int MessageLockTime { get; set; }


        /// <summary>
        /// 消息处理失败按发生异常处理
        /// </summary>

        public bool FailedIsError { get; set; }

        /// <summary>
        /// 无处理方法按发生异常处理
        /// </summary>

        public bool NoSupperIsError { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly RedisOption Instance = new RedisOption();


        static RedisOption()
        {
            ConfigurationManager.RegistOnChange("MessageMVC:Redis",Instance.Update, true);
        }

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        private void Update()
        {
            RedisOption option = ConfigurationManager.Option<RedisOption>("MessageMVC:Redis") ;

            ConnectionString = option.ConnectionString;
            GuardCheckTime = option.GuardCheckTime;
            MessageLockTime = option.MessageLockTime;
            FailedIsError = option.FailedIsError;
            NoSupperIsError = option.NoSupperIsError;

            if (GuardCheckTime <= 0)
            {
                GuardCheckTime = 3000;
            }

            if (MessageLockTime <= 0)
            {
                MessageLockTime = 1000;
            }
        }
    }
}
