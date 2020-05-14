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
        ///  唯一实例 
        /// </summary>
        public readonly static KafkaOption Instance = new KafkaOption
        {
            TestTopic = "HealthCheck"
        };

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
            Producer.BootstrapServers = BootstrapServers;
            Consumer.BootstrapServers = BootstrapServers;

            ConsumerLoad(Consumer);
            ProducerLoad();
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
        void ProducerLoad()
        {
            ConfigLoad(Producer);
            var config = ConfigurationHelper.Get<ProducerConfig>(ProducerName);
            if (config == null)
                return;
            Producer.QueueBufferingBackpressureThreshold = config.QueueBufferingBackpressureThreshold;
            Producer.RetryBackoffMs = config.RetryBackoffMs;
            Producer.MessageSendMaxRetries = config.MessageSendMaxRetries;
            Producer.LingerMs = config.LingerMs;
            Producer.QueueBufferingMaxKbytes = config.QueueBufferingMaxKbytes;
            Producer.QueueBufferingMaxMessages = config.QueueBufferingMaxMessages;
            Producer.EnableGaplessGuarantee = config.EnableGaplessGuarantee;
            Producer.CompressionLevel = config.CompressionLevel;
            Producer.CompressionType = config.CompressionType;
            Producer.Partitioner = config.Partitioner;
            Producer.MessageTimeoutMs = config.MessageTimeoutMs;
            Producer.RequestTimeoutMs = config.RequestTimeoutMs;
            Producer.DeliveryReportFields = config.DeliveryReportFields;
            Producer.EnableDeliveryReports = config.EnableDeliveryReports;
            Producer.EnableBackgroundPoll = config.EnableBackgroundPoll;
            Producer.EnableIdempotence = config.EnableIdempotence;
            Producer.BatchNumMessages = config.BatchNumMessages;
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
    }
}

