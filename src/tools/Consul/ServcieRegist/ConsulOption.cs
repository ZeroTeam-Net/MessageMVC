using Agebull.Common.Configuration;
using Consul;
using System;

namespace ZeroTeam.MessageMVC.Consul
{
    /// <summary>
    /// Consul对应配置
    /// </summary>
    internal class ConsulOption
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
        public static ConsulOption Instance = new ConsulOption
        {
            HealthName = "_health_/consul",
            RegisterAfter = 1,
            HealthInterval = 10,
            HealthTimeout = 3
        };

        static ConsulOption()
        {
            ConfigurationHelper.RegistOnChange<ConsulOption>("Consul", Instance.OnChange, true);
        }

        private void OnChange(ConsulOption option)
        {
            IP = option.IP ?? "127.0.0.1";
            Port = option.Port;
            ConsulIP = option.ConsulIP ?? "127.0.0.1";
            ConsulPort = option.ConsulPort <= 0 ? 8500 : option.ConsulPort;
            if (option.RegisterAfter > 0)
                RegisterAfter = option.RegisterAfter;
            if (option.HealthInterval > 0)
                HealthInterval = option.HealthInterval;
            if (option.HealthTimeout > 0)
                HealthTimeout = option.HealthTimeout;
            serviceCheck = null;
        }

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