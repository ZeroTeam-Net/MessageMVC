using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.RedisMQ
{
    /// <summary>
    /// Redis的配置项
    /// </summary>
    internal class RedisOption: IZeroOption
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
        /// 是否异步发送
        /// </summary>
        public bool AsyncPost { get; set; }

        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly RedisOption Instance = new();

        const string sectionName = "MessageMVC:Redis";

        const string optionName = "Redis发布订阅配置";

        const string supperUrl = "https://";

        /// <summary>
        /// 支持地址
        /// </summary>
        string IZeroOption.SupperUrl => supperUrl;

        /// <summary>
        /// 配置名称
        /// </summary>
        string IZeroOption.OptionName => optionName;


        /// <summary>
        /// 节点名称
        /// </summary>
        string IZeroOption.SectionName => sectionName;

        /// <summary>
        /// 是否动态配置
        /// </summary>
        bool IZeroOption.IsDynamic => false;

        void IZeroOption.Load(bool first)
        {
            var option = ConfigurationHelper.Get<RedisOption>(sectionName);
            if (option == null)
                throw new ZeroOptionException(optionName, sectionName);
            if (option.ConnectionString.IsMissing())
                throw new ZeroOptionException(optionName, sectionName, "ConnectionString必须配置");
            if (!string.IsNullOrWhiteSpace(option.ConnectionString))
            {
                ConnectionString = option.ConnectionString;
            }

            AsyncPost = option.AsyncPost;
            FailedIsError = option.FailedIsError;
            NoSupperIsError = option.NoSupperIsError;

            GuardCheckTime = option.GuardCheckTime;
            if (GuardCheckTime <= 0)
            {
                GuardCheckTime = 3000;
            }

            MessageLockTime = option.MessageLockTime;
            if (MessageLockTime <= 0)
            {
                MessageLockTime = 1000;
            }
        }
        #endregion
    }
}
