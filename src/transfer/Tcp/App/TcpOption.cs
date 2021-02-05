using Agebull.Common.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace ZeroTeam.MessageMVC.Tcp
{

    /// <summary>
    /// 接收服务配置
    /// </summary>
    public class ClientOption
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否日志
        /// </summary>
        public bool IsLog { get; set; }

    }
    /// <summary>
    /// 接收服务配置
    /// </summary>
    public class ServerOption
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 同时处理数据最大并发数(1-8192)，默认为32
        /// </summary>
        public int Concurrency { get; set; }

        /// <summary>
        /// 服务类型，默认为RPC
        /// </summary>
        public Dictionary<string, string> ServiceTypes { get; set; }
    }

    /// <summary>
    /// 接收服务配置
    /// </summary>
    public class TcpOption : IZeroOption
    {
        /// <summary>
        /// 服务端
        /// </summary>
        public ServerOption Server { get; set; }

        /// <summary>
        /// 客户端
        /// </summary>
        public ClientOption Client { get; set; }

        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly TcpOption Instance = new TcpOption();

        internal static bool haseConsumer, haseProducer;

        const string sectionName = "MessageMVC:Tcp";

        const string ClientName = "MessageMVC:Tcp:Client";

        const string ServerName = "MessageMVC:Tcp:Server";

        const string optionName = "Tcp配置";

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
            Server = ConfigurationHelper.Get<ServerOption>(ServerName);
            Client = ConfigurationHelper.Get<ClientOption>(ClientName);
            if (haseConsumer && Server == null)
                throw new ZeroOptionException(optionName, ServerName);

            if (haseProducer && Client == null)
                throw new ZeroOptionException(optionName, ServerName);
            if (Server == null)
                return;
            if (Server.Concurrency <= 0)
                Server.Concurrency = 0;
            else if (Server.Concurrency > 1024)
                Server.Concurrency = 1024;

            ConcurrencySemaphore = new SemaphoreSlim(Server.Concurrency, Server.Concurrency);
        }

        #endregion

        /// <summary>
        /// 并发数量控制信号量
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public SemaphoreSlim ConcurrencySemaphore { get; private set; }

        /// <summary>
        /// 服务是否指定类型
        /// </summary>
        /// <param name="service"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(string service, string type = "Rpc")
        {
            if (Server?.ServiceTypes == null || !Server.ServiceTypes.TryGetValue(service, out var myType))
            {
                return type == "Rpc";
            }
            return type == myType;
        }
    }
}
