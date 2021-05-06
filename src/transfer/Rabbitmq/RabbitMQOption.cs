using Agebull.Common.Configuration;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.RabbitMQ
{
    /// <summary>
    /// RabbitMQ配置
    /// </summary>
    public class RabbitMQOption : IZeroOption
    {
        /// <summary>
        ///  唯一实例 
        /// </summary>
        public readonly static RabbitMQOption Instance = new();

        #region 基础配置

        /// <summary>
        /// The host to connect to.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The port to connect on. RabbitMQ.Client.AmqpTcpEndpoint.UseDefaultPort indicates. the default for the protocol should be used.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Username to use when authenticating to the server.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password to use when authenticating to the server.
        /// </summary>
        public string Password { get; set; }

        #endregion

        #region 高级配置

        /*// <summary>
        /// Timeout setting for connection attempts.
        /// </summary>
        public TimeSpan RequestedConnectionTimeout { get; set; }
        //
        // 摘要:
        //     Timeout setting for socket read operations.
        public TimeSpan SocketReadTimeout { get; set; }
        //
        // 摘要:
        //     Timeout setting for socket write operations.
        public TimeSpan SocketWriteTimeout { get; set; }
        //
        // 摘要:
        //     TLS options setting.
        public SslOption Ssl { get; set; }
        //
        // 摘要:
        //     Set to false to make automatic connection recovery not recover topology (exchanges,
        //     queues, bindings, etc). Defaults to true.
        public bool TopologyRecoveryEnabled { get; set; }
        //
        // 摘要:
        //     Connection endpoint.
        public AmqpTcpEndpoint Endpoint { get; set; }
        //
        // 摘要:
        //     Frame-max parameter to ask for (in bytes).
        public uint RequestedFrameMax { get; set; }
        //
        // 摘要:
        //     Maximum channel number to ask for.
        public ushort RequestedChannelMax { get; set; }
        
        //
        // 摘要:
        //     Heartbeat timeout to use when negotiating with the server.
        public TimeSpan RequestedHeartbeat { get; set; }
        //
        // 摘要:
        //     When set to true, background thread will be used for the I/O loop.
        public bool UseBackgroundThreadsForIO { get; set; }

        //
        // 摘要:
        //     Virtual host to access during this connection.
        public string VirtualHost { get; set; }
        //
        // 摘要:
        //     Dictionary of client properties to be sent to the server.
        public Dictionary<string, object> ClientProperties { get; set; }
        //
        // 摘要:
        //     Factory function for creating the RabbitMQ.Client.IEndpointResolver used to generate
        //     a list of endpoints for the ConnectionFactory to try in order. The default value
        //     creates an instance of the RabbitMQ.Client.DefaultEndpointResolver using the
        //     list of endpoints passed in. The DefaultEndpointResolver shuffles the provided
        //     list each time it is requested.
        public Func<IEnumerable<AmqpTcpEndpoint>, IEndpointResolver> EndpointResolverFactory { get; set; }
        //
        // 摘要:
        //     Set to true will enable a asynchronous consumer dispatcher which is compatible
        //     with RabbitMQ.Client.IAsyncBasicConsumer. Defaults to false.
        public bool DispatchConsumersAsync { get; set; }
        //
        // 摘要:
        //     Amount of time protocol handshake operations are allowed to take before timing
        //     out.
        public TimeSpan HandshakeContinuationTimeout { get; set; }
        //
        // 摘要:
        //     Amount of time protocol operations (e.g.
        //     queue.declare
        //     ) are allowed to take before timing out.
        public TimeSpan ContinuationTimeout { get; set; }
        //
        // 摘要:
        //     The AMQP URI SSL protocols.
        public SslProtocols AmqpUriSslProtocols { get; set; }
        //
        // 摘要:
        //     SASL auth mechanisms to use.
        public IList<IAuthMechanismFactory> AuthMechanisms { get; set; }
        //
        // 摘要:
        //     Default client provided name to be used for connections.
        public string ClientProvidedName { get; set; }
        //
        // 摘要:
        //     The uri to use for the connection.
        public Uri Uri { get; set; }
        //
        // 摘要:
        //     Amount of time client will wait for before re-trying to recover connection.
        public TimeSpan NetworkRecoveryInterval { get; set; }

        /// <summary>
        /// Set to false to disable automatic connection recovery. Defaults to true.
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; }
        */

        #endregion

        #region 队列使用

        /// <summary>
        /// 节点配置
        /// </summary>
        public Dictionary<string, RabbitMQItemOption> ItemOptions { get; set; }

        /// <summary>
        /// 同时处理数据最大并发数
        /// </summary>
        public int Concurrency { get; set; }

        /// <summary>
        /// 是否异步发送
        /// </summary>
        public bool AsyncPost { get; set; }

        #endregion

        #region 配置自动更新

        const string sectionName = "MessageMVC:RabbitMQ";


        const string optionName = "RabbitMQ发布订阅配置";

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
            var option = ConfigurationHelper.Get<RabbitMQOption>(sectionName);
            if (option == null)
                throw new ZeroOptionException(optionName, sectionName);
            if (option.HostName.IsMissing() || option.Port < 0 || option.Port > 65536)
                throw new ZeroOptionException(optionName, sectionName, "HostName或Port不正确");

            HostName = option.HostName;
            Port = option.Port;
            UserName = option.UserName;
            Password = option.Password;
            AsyncPost = option.AsyncPost;
            Concurrency = option.Concurrency;
            if (Concurrency <= 0)
                Concurrency = 1;
            if (ItemOptions == null)
                ItemOptions = new Dictionary<string, RabbitMQItemOption>(StringComparer.OrdinalIgnoreCase);
            if (option.ItemOptions == null || option.ItemOptions.Count == 0)
                return;
            foreach (var kv in option.ItemOptions)
            {
                if (!ItemOptions.ContainsKey(kv.Key))
                    ItemOptions.Add(kv.Key, kv.Value);
            }
        }
        #endregion
    }

    /// <summary>
    /// 工作模式
    /// </summary>
    public enum RabbitMQWrokType
    {
        /// <summary>
        /// 缺省模式（无Exchange）
        /// </summary>
        Default,
        /// <summary>
        /// 发布订阅模式
        /// </summary>
        Fanout,
        /// <summary>
        /// 路由模式
        /// </summary>
        Direct,
        /// <summary>
        /// 通配符模式
        /// </summary>
        Topic
    }

    /// <summary>
    /// RabbitMQ节点配置
    /// </summary>
    public class RabbitMQItemOption
    {

        /// <summary>
        /// 工作模式
        /// </summary>
        public RabbitMQWrokType WrokType { get; set; }

        /// <summary>
        /// 并行消费
        /// </summary>
        public ushort Qos { get; set; }


        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// 是否成功后确认Ack
        /// </summary>
        public bool AckBySuccess { get; set; }


        /// <summary>
        /// 自动删除
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// 排外
        /// </summary>
        public bool Exclusive { get; set; }

        /// <summary>
        /// 交换机
        /// </summary>
        public string ExchangeName { get; set; }
    }
}

