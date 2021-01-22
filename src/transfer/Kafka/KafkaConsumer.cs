using Agebull.Common.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ConsumeResult = Confluent.Kafka.ConsumeResult<Confluent.Kafka.Ignore, string>;

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

        private IConsumer<Ignore, string> consumer;


        async Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (KafkaOption.Instance.Concurrency > 0)
                        await ConcurrencySemaphore.WaitAsync(token);
                    var cr = consumer.Consume(token);
                    if (cr == null)
                    {
                        continue;
                    }
                    if (cr.IsPartitionEOF)
                    {
                        continue;
                    }
                    IInlineMessage message;
                    try
                    {
                        if (!SmartSerializer.TryToMessage(cr.Message.Value, out message))
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Exception(e);
                        continue;
                    }

                    if (KafkaOption.Instance.Concurrency <= 0)
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
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "KafkaConsumer.Loop");
                }
            }
            return true;
        }


        private ConsumerBuilder<Ignore, string> builder;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            builder = new ConsumerBuilder<Ignore, string>(KafkaOption.Instance.Consumer);
            builder.SetErrorHandler(OnError);
            builder.SetLogHandler(OnLog);
            consumer = builder.Build();

            consumer.Subscribe(Service.ServiceName);

            if (KafkaOption.Instance.Concurrency > 0)
                ConcurrencySemaphore = new SemaphoreSlim(KafkaOption.Instance.Concurrency, KafkaOption.Instance.Concurrency);
            return Task.FromResult(true);
        }
        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.LoopComplete()
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
        Task<bool> IMessageReceiver.OnResult(IInlineMessage item, object tag)
        {
            if (KafkaOption.Instance.Concurrency > 0)
                ConcurrencySemaphore.Release();

            var consumeResult = (ConsumeResult)tag;
            try
            {
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

        void OnError(IConsumer<Ignore, string> consumer, Error error)
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
        void OnLog(IConsumer<Ignore, string> consumer, LogMessage msg)
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
