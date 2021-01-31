using Agebull.Common.Configuration;
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

    }
    /// <summary>
    /// 接收服务配置
    /// </summary>
    public class TcpOption
    {
        /// <summary>
        /// 服务端
        /// </summary>
        public ServerOption Server { get; set; }

        /// <summary>
        /// 客户端
        /// </summary>
        public ClientOption Client { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly TcpOption Instance = new TcpOption();



        internal void LoadOption()
        {
            Server = ConfigurationHelper.Get<ServerOption>("MessageMVC:Tcp:Server");
            Client = ConfigurationHelper.Get<ClientOption>("MessageMVC:Tcp:Client");
            if (Server == null)
                return;
            if (Server.Concurrency <= 0)
                Server.Concurrency = 0;
            else if(Server.Concurrency > 1024)
                Server.Concurrency = 1024;

            ConcurrencySemaphore = new SemaphoreSlim(Server.Concurrency, Server.Concurrency);
        }
        
        /// <summary>
        /// 并发数量控制信号量
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public SemaphoreSlim ConcurrencySemaphore { get; private set; }

    }
}
