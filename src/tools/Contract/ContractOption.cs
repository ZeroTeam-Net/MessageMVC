using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.ApiContract
{
    /// <summary>
    /// 扩展工具配置
    /// </summary>
    internal class ContractOption
    {
        /// <summary>
        ///     启用返回值调用链跟踪
        /// </summary>

        public bool EnableResultTrace { get; set; }

        /// <summary>
        ///     返回值跟踪包含机器名
        /// </summary>

        public bool TraceMachine { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ContractOption Instance = new ContractOption();

        static ContractOption()
        {
            ConfigurationHelper.RegistOnChange<ContractOption>("MessageMVC:ApiContract", Instance.Update, true);
        }

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        private void Update(ContractOption option)
        {
            TraceMachine = option.TraceMachine;
            EnableResultTrace = option.EnableResultTrace;
        }
    }
}
