using Agebull.Common.Configuration;
using Consul;
using System;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    /// Consul对应配置
    /// </summary>
    internal class ConsulOption:IZeroOption
    {
        /// <summary>
        /// 本地IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Consol的IP
        /// </summary>
        public string ConsulIP { get; set; }

        /// <summary>
        /// Consol的端口
        /// </summary>
        public int ConsulPort { get; set; }

        /// <summary>
        /// 健康检查接口名称
        /// </summary>
        public string HealthName { get; set; }

        /// <summary>
        /// 服务启动多久后注册(单位秒),默认为1秒
        /// </summary>
        public int RegisterAfter { get; set; }

        /// <summary>
        /// 健康检查时间间隔，或者称为心跳间隔(单位秒),默认为10秒
        /// </summary>
        public int HealthInterval { get; set; }

        /// <summary>
        /// 健康检查超时时长(单位秒),默认为3秒
        /// </summary>
        public int HealthTimeout { get; set; }

        /// <summary>
        /// 唯一实例
        /// </summary>
        public static ConsulOption Instance = new()
        {
            HealthName = "_health_/consul",
            RegisterAfter = 1,
            HealthInterval = 10,
            HealthTimeout = 3
        };

        #region IZeroOption


        const string sectionName = "Consul";

        const string optionName = "HttpClient配置";

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
            var option = ConfigurationHelper.Get<ConsulOption>(sectionName);
            if (option == null)
                return;
            IP = option.IP ?? ZeroAppOption.Instance.LocalIpAddress;
            Port = option.Port;
            ConsulIP = option.ConsulIP;
            ConsulPort = option.ConsulPort <= 0 ? 8500 : option.ConsulPort;
            if (option.RegisterAfter > 0)
                RegisterAfter = option.RegisterAfter;
            if (option.HealthInterval > 0)
                HealthInterval = option.HealthInterval;
            if (option.HealthTimeout > 0)
                HealthTimeout = option.HealthTimeout;
        }
        #endregion

        AgentServiceCheck serviceCheck;

        /// <summary>
        /// 代理服务检查配置
        /// </summary>
        public AgentServiceCheck AgentServiceCheck => serviceCheck ??= new AgentServiceCheck()
        {
            HTTP = $"http://{IP}:{Port}/{HealthName}",//健康检查地址
            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(RegisterAfter),
            Interval = TimeSpan.FromSeconds(HealthInterval),
            Timeout = TimeSpan.FromSeconds(HealthTimeout)
        };
    }
}