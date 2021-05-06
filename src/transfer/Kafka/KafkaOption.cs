using Agebull.Common.Configuration;
using Confluent.Kafka;
using System;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    /// 消息配置
    /// </summary>
    public class MessageOption
    {
        /// <summary>
        /// 同时处理数据最大并发数
        /// </summary>
        public int Concurrency { get; set; }

        /// <summary>
        /// 测试主题
        /// </summary>
        public string TestTopic { get; set; }

        /// <summary>
        /// 是否异步发送
        /// </summary>
        public bool AsyncPost { get; set; }

    }

    /// <summary>
    /// Kafka配置
    /// </summary>
    public class KafkaOption : IZeroOption
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string BootstrapServers { get; set; }

        /// <summary>
        /// 消费配置
        /// </summary>
        public ConsumerConfig Consumer { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public ProducerConfig Producer { get; set; }

        /// <summary>
        /// 消息配置
        /// </summary>
        public MessageOption Message { get; set; }

        #region IZeroOption


        /// <summary>
        ///  唯一实例 
        /// </summary>
        public readonly static KafkaOption Instance = new();

        internal static bool haseConsumer, haseProducer;

        const string sectionName = "MessageMVC:Kafka";

        const string ClientName = "MessageMVC:Kafka:Client";

        const string ConsumerName = "MessageMVC:Kafka:Consumer";

        const string ProducerName = "MessageMVC:Kafka:Producer";

        const string MessageName = "MessageMVC:Kafka:Message";

        const string optionName = "Kafka发布订阅配置";

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
            BootstrapServers = ConfigurationHelper.Get(sectionName).GetStr("BootstrapServers", BootstrapServers);

            LoadMessage();
            if (haseConsumer)
            {
                LoadConsumer();
                if (Consumer.BootstrapServers.IsMissing() || Consumer.GroupId.IsMissing())
                    throw new ZeroOptionException(optionName, sectionName, "Consumer.BootstrapServers与Consumer.GroupId不能为空");
            }
            if (haseProducer)
            {
                LoadProducer();
                if (Producer.BootstrapServers.IsMissing())
                    throw new ZeroOptionException(optionName, sectionName, "Producer.BootstrapServers不能为空");
            }
        }
        internal void LoadMessage()
        {
            Message = new MessageOption
            {
                TestTopic = "HealthCheck",
                Concurrency = 128
            };
            var config = ConfigurationHelper.Get<MessageOption>(MessageName);
            if (config == null)
            {
                return;
            }
            Message.TestTopic = config.TestTopic;
            Message.Concurrency = config.Concurrency;
            Message.AsyncPost = config.AsyncPost;
        }
        bool LoadProducer()
        {
            var producer = Producer = new ProducerConfig();
            var config = ConfigurationHelper.Get<ProducerConfig>(ProducerName);
            if (config == null)
            {
                return false;
            }

            LoadClient(producer);
            producer.BootstrapServers = config.BootstrapServers ?? BootstrapServers;
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
            return true;
        }

        bool LoadConsumer()
        {
            var consumer = Consumer = new ConsumerConfig();
            var config = ConfigurationHelper.Get<ConsumerConfig>(ConsumerName);
            if (config == null)
                return false;
            LoadClient(consumer);
            consumer.BootstrapServers = config.BootstrapServers ?? BootstrapServers;
            consumer.IsolationLevel = config.IsolationLevel;
            consumer.FetchErrorBackoffMs = config.FetchErrorBackoffMs;
            consumer.FetchMinBytes = config.FetchMinBytes;
            consumer.FetchMaxBytes = config.FetchMaxBytes;
            consumer.FetchWaitMaxMs = config.FetchWaitMaxMs;
            consumer.QueuedMaxMessagesKbytes = config.QueuedMaxMessagesKbytes;
            consumer.QueuedMinMessages = config.QueuedMinMessages;
            consumer.EnableAutoOffsetStore = config.EnableAutoOffsetStore;
            consumer.AutoCommitIntervalMs = config.AutoCommitIntervalMs;
            consumer.EnableAutoCommit = config.EnableAutoCommit;
            consumer.MaxPollIntervalMs = config.MaxPollIntervalMs;
            consumer.GroupProtocolType = config.GroupProtocolType;
            consumer.SessionTimeoutMs = config.SessionTimeoutMs;
            consumer.GroupId = config.GroupId;
            consumer.AutoOffsetReset = config.AutoOffsetReset;
            consumer.CheckCrcs = config.CheckCrcs;
            return true;
        }

        static void LoadClient(ClientConfig dest)
        {
            var config = ConfigurationHelper.Get<ClientConfig>(ClientName);
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

