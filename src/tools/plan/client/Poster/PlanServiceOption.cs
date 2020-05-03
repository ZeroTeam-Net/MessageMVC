using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划任务服务配置
    /// </summary>
    public class PlanServiceOption
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 投递接口名称
        /// </summary>
        public string PostApiName { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public readonly static PlanServiceOption Instance = new PlanServiceOption
        {
            ServiceName = "PlanTask",
            PostApiName = "v1/post"
        };


        /// <summary>
        /// 构造
        /// </summary>
        static PlanServiceOption()
        {
            ConfigurationManager.RegistOnChange<PlanServiceOption>("MessageMVC:PlanService", Load, true);
        }


        /// <summary>
        /// 构造
        /// </summary>
        static void Load(PlanServiceOption option)
        {
            if (string.IsNullOrEmpty(option.ServiceName))
            {
                Instance.ServiceName = option.ServiceName;
            }

            if (string.IsNullOrEmpty(option.PostApiName))
            {
                Instance.PostApiName = "post";
            }
        }
    }

}