using Agebull.Common.Configuration;
using Confluent.Kafka;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    /// Kafka配置
    /// </summary>
    public class KafkaOption
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string BootstrapServers { get; set; }

        /// <summary>
        /// 测试主题
        /// </summary>
        public string TestTopic { get; set; }

        /// <summary>
        /// 消费配置
        /// </summary>
        public ConsumerConfig Consumer = new ConsumerConfig();

        /// <summary>
        /// 服务地址
        /// </summary>
        public ProducerConfig Producer = new ProducerConfig();


        /// <summary>
        /// 同时处理数据最大并发数
        /// </summary>
        public int Concurrency { get; set; }

        /// <summary>
        ///  唯一实例 
        /// </summary>
        public readonly static KafkaOption Instance = new KafkaOption
        {
            TestTopic = "HealthCheck"
        };

        #region 配置自动更新

        const string sectionName = "MessageMVC:Kafka";

        const string ConfigName = "MessageMVC:Kafka:Client";

        const string ConsumerName = "MessageMVC:Kafka:Consumer";

        const string ProducerName = "MessageMVC:Kafka:Producer";

        static KafkaOption()
        {
            ConfigurationHelper.RegistOnChange(sectionName, Instance.Load, true);
        }

        void Load()
        {
            BootstrapServers = ConfigurationHelper.Get(sectionName).GetStr("BootstrapServers", BootstrapServers);
            TestTopic = ConfigurationHelper.Get(sectionName).GetStr("TestTopic", TestTopic);
            Concurrency = ConfigurationHelper.Get(sectionName).GetInt("Concurrency", 0);

            ConsumerLoad(Consumer);
            Consumer.BootstrapServers = BootstrapServers;
            ProducerLoad(Producer);
            Producer.BootstrapServers = BootstrapServers;
        }
        internal ConsumerConfig CopyConsumer()
        {
            var cfg = new ConsumerConfig
            {
                BootstrapServers = BootstrapServers
            };
            ConsumerLoad(cfg);
            return cfg;
        }
        static void ProducerLoad(ProducerConfig producer)
        {
            ConfigLoad(producer);
            var config = ConfigurationHelper.Get<ProducerConfig>(ProducerName);
            if (config == null)
                return;
            producer.QueueBufferingBackpressureThreshold = config.QueueBufferingBackpressureThreshold;
            producer.RetryBackoffMs = config.RetryBackoffMs;
            producer.MessageSendMaxRetries = config.MessageSendMaxRetries;
            producer.LingerMs = config.LingerMs;
            producer.QueueBufferingMaxKbytes = config.QueueBufferingMaxKbytes;
            producer.QueueBufferingMaxMessages = config.QueueBufferingMaxMessages;
            producer.EnableGaplessGuarantee = config.EnableGaplessGuarantee;
            producer.CompressionLevel = config.CompressionLevel;
            producer.CompressionType = config.CompressionType;
            producer.Partitioner = config.Partitioner;
            producer.MessageTimeoutMs = config.MessageTimeoutMs;
            producer.RequestTimeoutMs = config.RequestTimeoutMs;
            if (!string.IsNullOrWhiteSpace(config.DeliveryReportFields))
                producer.DeliveryReportFields = config.DeliveryReportFields;
            producer.EnableDeliveryReports = config.EnableDeliveryReports;
            producer.EnableBackgroundPoll = config.EnableBackgroundPoll;
            producer.EnableIdempotence = config.EnableIdempotence;
            producer.BatchNumMessages = config.BatchNumMessages;
        }

        static void ConsumerLoad(ConsumerConfig con)
        {
            ConfigLoad(con);
            var config = ConfigurationHelper.Get<ConsumerConfig>(ConsumerName);
            if (config == null)
                return;
            con.IsolationLevel = config.IsolationLevel;
            con.FetchErrorBackoffMs = config.FetchErrorBackoffMs;
            con.FetchMinBytes = config.FetchMinBytes;
            con.FetchMaxBytes = config.FetchMaxBytes;
            con.FetchWaitMaxMs = config.FetchWaitMaxMs;
            con.QueuedMaxMessagesKbytes = config.QueuedMaxMessagesKbytes;
            con.QueuedMinMessages = config.QueuedMinMessages;
            con.EnableAutoOffsetStore = config.EnableAutoOffsetStore;
            con.AutoCommitIntervalMs = config.AutoCommitIntervalMs;
            con.EnableAutoCommit = config.EnableAutoCommit;
            con.MaxPollIntervalMs = config.MaxPollIntervalMs;
            con.GroupProtocolType = config.GroupProtocolType;
            con.SessionTimeoutMs = config.SessionTimeoutMs;
            con.GroupId = config.GroupId;
            con.AutoOffsetReset = config.AutoOffsetReset;
            con.CheckCrcs = config.CheckCrcs;
        }

        static void ConfigLoad(ClientConfig dest)
        {

            var config = ConfigurationHelper.Get<ClientConfig>(ConfigName);
            if (config == null)
                return;

            dest.ApiVersionFallbackMs = config.ApiVersionFallbackMs;
            dest.BrokerVersionFallback = config.BrokerVersionFallback;
            dest.SecurityProtocol = config.SecurityProtocol;
            dest.SslCipherSuites = config.SslCipherSuites;
            dest.SslCurvesList = config.SslCurvesList;
            dest.SslSigalgsList = config.SslSigalgsList;
            dest.SslKeyLocation = config.SslKeyLocation;
            dest.SslKeyPassword = config.SslKeyPassword;
            dest.SslKeyPem = config.SslKeyPem;
            dest.SslCertificatePem = config.SslCertificatePem;
            dest.SslKeystoreLocation = config.SslKeystoreLocation;
            dest.ApiVersionRequest = config.ApiVersionRequest;

            dest.SslKeystorePassword = config.SslKeystorePassword;
            dest.EnableSslCertificateVerification = config.EnableSslCertificateVerification;
            dest.SslEndpointIdentificationAlgorithm = config.SslEndpointIdentificationAlgorithm;
            dest.SaslKerberosServiceName = config.SaslKerberosServiceName;
            dest.SaslKerberosPrincipal = config.SaslKerberosPrincipal;
            dest.SaslKerberosKinitCmd = config.SaslKerberosKinitCmd;
            dest.SaslKerberosKeytab = config.SaslKerberosKeytab;
            dest.SaslKerberosMinTimeBeforeRelogin = config.SaslKerberosMinTimeBeforeRelogin;
            dest.SaslUsername = config.SaslUsername;
            dest.SaslPassword = config.SaslPassword;
            dest.SaslOauthbearerConfig = config.SaslOauthbearerConfig;
            dest.EnableSaslOauthbearerUnsecureJwt = config.EnableSaslOauthbearerUnsecureJwt;
            dest.SslCrlLocation = config.SslCrlLocation;
            dest.InternalTerminationSignal = config.InternalTerminationSignal;
            dest.LogConnectionClose = config.LogConnectionClose;
            dest.LogThreadName = config.LogThreadName;
            dest.SaslMechanism = config.SaslMechanism;
            dest.Acks = config.Acks;
            dest.ClientId = config.ClientId;
            dest.MessageMaxBytes = config.MessageMaxBytes;
            dest.MessageCopyMaxBytes = config.MessageCopyMaxBytes;
            dest.ReceiveMessageMaxBytes = config.ReceiveMessageMaxBytes;
            dest.MaxInFlight = config.MaxInFlight;
            dest.MetadataRequestTimeoutMs = config.MetadataRequestTimeoutMs;
            dest.TopicMetadataRefreshIntervalMs = config.TopicMetadataRefreshIntervalMs;
            dest.MetadataMaxAgeMs = config.MetadataMaxAgeMs;
            dest.TopicMetadataRefreshFastIntervalMs = config.TopicMetadataRefreshFastIntervalMs;
            dest.TopicMetadataRefreshSparse = config.TopicMetadataRefreshSparse;
            dest.TopicBlacklist = config.TopicBlacklist;
            dest.Debug = config.Debug;
            dest.SocketTimeoutMs = config.SocketTimeoutMs;
            dest.SocketSendBufferBytes = config.SocketSendBufferBytes;
            dest.SocketReceiveBufferBytes = config.SocketReceiveBufferBytes;
            dest.SocketKeepaliveEnable = config.SocketKeepaliveEnable;
            dest.SocketNagleDisable = config.SocketNagleDisable;
            dest.SocketMaxFails = config.SocketMaxFails;
            dest.BrokerAddressTtl = config.BrokerAddressTtl;
            dest.BrokerAddressFamily = config.BrokerAddressFamily;
            dest.ReconnectBackoffMs = config.ReconnectBackoffMs;
            dest.ReconnectBackoffMaxMs = config.ReconnectBackoffMaxMs;
            dest.StatisticsIntervalMs = config.StatisticsIntervalMs;
            dest.LogQueue = config.LogQueue;
            dest.PluginLibraryPaths = config.PluginLibraryPaths;
            dest.ClientRack = config.ClientRack;

        }
        #endregion
    }
}

