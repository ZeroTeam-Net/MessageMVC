using Agebull.Common.Logging;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ConsumeResult = Confluent.Kafka.ConsumeResult<byte[], string>;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    /// Kafka消息队列消费者
    /// </summary>
    internal class KafkaConsumer : MessageReceiverBase, IMessageConsumer
    {
        SemaphoreSlim ConcurrencySemaphore;

        /// <summary>
        /// 构造
        /// </summary>
        public KafkaConsumer() : base(nameof(KafkaConsumer))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(KafkaPoster);

        private IConsumer<byte[], string> consumer;


        async Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (KafkaOption.Instance.Message.Concurrency > 0)
                        await ConcurrencySemaphore.WaitAsync(token);
                    var cr = consumer.Consume(token);
                    if (cr == null)
                    {
                        if (KafkaOption.Instance.Message.Concurrency > 0)
                            ConcurrencySemaphore.Release();
                        continue;
                    }
                    if (cr.IsPartitionEOF)
                    {
                        if (KafkaOption.Instance.Message.Concurrency > 0)
                            ConcurrencySemaphore.Release();
                        continue;
                    }
                    if (cr.Message.Key == null)
                    {
                        if (KafkaOption.Instance.Message.Concurrency > 0)
                            ConcurrencySemaphore.Release();
                        consumer.Commit(cr);
                        continue;
                    }
                    var message = SmartSerializer.ToObject<InlineMessage>(cr.Message.Key);
                    message.Service = cr.Topic;
                    message.Argument = cr.Message.Value;
                    //try
                    //{
                    //    foreach (var header in cr.Message.Headers)
                    //    {
                    //        byte[] bytes = header.GetValueBytes();
                    //        if (bytes == null)
                    //            continue;
                    //        switch (header.Key)
                    //        {
                    //            case "zid":
                    //                message.ID = Encoding.ASCII.GetString(bytes);
                    //                break;
                    //            case "method":
                    //                message.Method = Encoding.ASCII.GetString(bytes);
                    //                break;
                    //            case "trace":
                    //                message.Trace = JsonSerializer.Deserialize<Context.TraceInfo>(bytes);
                    //                break;
                    //            case "ctx":
                    //                message.Context = JsonSerializer.Deserialize<Dictionary<string, string>>(bytes);
                    //                break;
                    //            case "user":
                    //                message.User = JsonSerializer.Deserialize<Dictionary<string, string>>(bytes);
                    //                break;
                    //        }
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    Logger.Exception(e);
                    //    continue;
                    //}

                    if (KafkaOption.Instance.Message.Concurrency <= 0)
                    {
                        await MessageProcessor.OnMessagePush(Service, message, true, cr);
                    }
                    else
                    {
                        MessageProcessor.RunOnMessagePush(Service, message, true, cr);
                    }
                }
                catch (OperationCanceledException)//取消为正常操作,不记录
                {
                    if (KafkaOption.Instance.Message.Concurrency > 0)
                        ConcurrencySemaphore.Release();
                    return true;
                }
                catch (Exception ex)
                {
                    if (KafkaOption.Instance.Message.Concurrency > 0)
                        ConcurrencySemaphore.Release();
                    Logger.Exception(ex, "KafkaConsumer.Loop");
                }
            }
            return true;
        }


        private ConsumerBuilder<byte[], string> builder;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            //KafkaOption.Instance.Consumer.Set(ConfigPropertyNames.Consumer.ConsumeResultFields, "all");
            builder = new ConsumerBuilder<byte[], string>(KafkaOption.Instance.Consumer);

            //var value = KafkaOption.Instance.Consumer.Get(ConfigPropertyNames.Consumer.ConsumeResultFields);

            builder.SetErrorHandler(OnError);
            builder.SetLogHandler(OnLog);
            consumer = builder.Build();

            consumer.Subscribe(Service.ServiceName);

            if (KafkaOption.Instance.Message.Concurrency > 0)
                ConcurrencySemaphore = new SemaphoreSlim(KafkaOption.Instance.Message.Concurrency, KafkaOption.Instance.Message.Concurrency);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task ILifeFlow.Close()
        {
            consumer.Close();
            consumer.Dispose();
            ConcurrencySemaphore?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageWriter.OnResult(IInlineMessage item, object tag)
        {
            var consumeResult = (ConsumeResult)tag;
            try
            {
                if (KafkaOption.Instance.Message.Concurrency > 0)
                    ConcurrencySemaphore.Release();

                if (item.State.IsEnd())
                    consumer.Commit(consumeResult);
                return Task.FromResult(true);
            }
            catch (KafkaException ex)
            {
                Logger.Exception(ex);
                return Task.FromResult(false);
            }
        }
        #region 日志

        void OnError(IConsumer<byte[], string> consumer, Error error)
        {/*
        //
        // 摘要:
        //     true if Code != ErrorCode.NoError.
        public bool IsError { get; }
        //
        // 摘要:
        //     Gets a human readable reason string associated with this error.
        public string Reason { get; }
        //
        // 摘要:
        //     Whether or not the error is fatal.
        public bool IsFatal { get; }
        //
        // 摘要:
        //     Gets the Confluent.Kafka.ErrorCode associated with this Error.
        public ErrorCode Code { get; }
        //
        // 摘要:
        //     true if this error originated on a broker, false otherwise.
        public bool IsBrokerError { get; }
        //
        // 摘要:
        //     true if this is error originated locally (within librdkafka), false otherwise.
        public bool IsLocalError { get; }*/
            //if(Logger.IsEnabled(LogLevel.Trace))
            //{
            //    Logger.Debug(() => $"【Kafka Error】 [IsError]{error.IsError} [IsFatal]{error.IsFatal} [Code]{error.Code} [IsBrokerError]{error.IsBrokerError} [IsLocalError]{error.IsLocalError}\n {error.Reason}");
            //}
            //if(error.IsLocalError)
            //{
            //    Logger.Error(() => $"【Kafka】本地网络异常({error.Code})\n {error.Reason}");
            //}
            //else
            //{
            //    Logger.Error(() => $"【Kafka】服务网络异常({error.Code})\n {error.Reason}");
            //}
        }
        void OnLog(IConsumer<byte[], string> consumer, LogMessage msg)
        {/*        //
        // 摘要:
        //     Gets the librdkafka client instance name.
        public string Name { get; }
        //
        // 摘要:
        //     Gets the log level (levels correspond to syslog(3)), lower is worse.
        public SyslogLevel Level { get; }
        //
        // 摘要:
        //     Gets the facility (section of librdkafka code) that produced the message.
        public string Facility { get; }
        //
        // 摘要:
        //     Gets the log message.
        public string Message { get; }*/


            //      Logger.Information(() => $"【Kafka log】 [Name]{msg.Name} [Level]{msg.Level} [Facility]{msg.Facility} [Message]{msg.Message}");
        }

        #endregion
    }
}
