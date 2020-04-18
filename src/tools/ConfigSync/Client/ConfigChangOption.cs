using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 配置更新配置
    /// </summary>
    public class ConfigChangOption
    {
        /// <summary>
        /// 唯一实例
        /// </summary>
        public static ConfigChangOption Instance = new ConfigChangOption();

        /// <summary>
        /// 静态构造
        /// </summary>
        static ConfigChangOption()
        {
            ConfigurationManager.RegistOnChange("MessageMVC:ConfigSync", () =>
            {
                var option = ConfigurationManager.Get<ConfigChangOption>("MessageMVC:ConfigSync");
                if(option != null)
                {
                    Instance.IsService = option.IsService;
                    Instance.ConnectionString = option.ConnectionString;
                }
            },true);
        }

        /// <summary>
        /// 是否服务
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// 配置的Redis键
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 配置的Redis键
        /// </summary>

        public const string ConfigRedisKey = "zt:mmvc:global:cfg";
    }


    /// <summary>
    /// HttpClient预定义服务映射配置
    /// </summary>
    public class HttpClientItem
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 基础地址,包含http://
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 绑定的服务列表,组合结果为 [Url]/[Service]/[ApiName]
        /// </summary>
        public string Services { get; set; }
    }
}
