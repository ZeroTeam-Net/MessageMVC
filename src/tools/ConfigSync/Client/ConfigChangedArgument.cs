namespace ZeroTeam.MessageMVC.ConfigSync
{

    /// <summary>
    /// 配置更新参数
    /// </summary>
    public class ConfigChangedArgument
    {
        /// <summary>
        /// 节点
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// 变更类型，section/value
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 键名
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// 配置更新配置
    /// </summary>
    public class ConfigChangOption
    {

        /// <summary>
        /// 配置的Redis键
        /// </summary>

        public const string ConfigRedisKey = "zt:mmvc:global:cfg";
    }
}
