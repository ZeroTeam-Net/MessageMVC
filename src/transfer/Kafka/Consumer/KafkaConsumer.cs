using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Confluent.Kafka;
using System;
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
        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, WaitCount, ErrorCount, SuccessCount;//, RecvCount, SendCount, SendError;


        /// <summary>
        /// 初始化
        /// </summary>
        bool IMessageReceiver.Prepare()
        {
            config = ConfigurationManager.Get<ConsumerConfig>("MessageMVC:Kafka");
            return config != null;
        }

        private ConsumerConfig config;
        private IConsumer<Ignore, string> consumer;


        async Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(token);
                    if (cr == null)
                    {
                        continue;
                    }
                    Interlocked.Increment(ref CallCount);
                    Interlocked.Increment(ref WaitCount);
                    MessageItem item;
                    try
                    {
                        item = JsonHelper.DeserializeObject<MessageItem>(cr.Value);
                        await OnMessagePush(item, cr);

                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        consumer.Commit();//无法处理的消息,直接确认
                        Interlocked.Increment(ref ErrorCount);
                        continue;
                    }
                }
                catch (OperationCanceledException)//取消为正常操作,不记录
                {
                    return true;
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex, "KafkaConsumer.Loop");
                }
            }
            return true;
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="message"></param>
        /// <param name="consumeResult"></param>
        private async Task OnMessagePush(IMessageItem message, ConsumeResult consumeResult)
        {
            await MessageProcessor.OnMessagePush(Service, message, consumeResult);//BUG:应该配置化同步或异步
            if (message.State == MessageState.Success)
            {
                Interlocked.Increment(ref SuccessCount);
            }
            else
            {
                Interlocked.Increment(ref ErrorCount);
            }

            Interlocked.Decrement(ref WaitCount);
        }

        private ConsumerBuilder<Ignore, string> builder;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            builder = new ConsumerBuilder<Ignore, string>(config);
            consumer = builder.Build();
            consumer.Subscribe(Service.ServiceName);
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
            return Task.CompletedTask;
        }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageReceiver.OnResult(IMessageItem item, object tag)
        {
            var consumeResult = (ConsumeResult)tag;
            try
            {
                consumer.Commit(consumeResult);
                return Task.FromResult(true);
            }
            catch (KafkaException ex)
            {
                LogRecorder.Exception(ex);
                return Task.FromResult(false);
            }
        }
    }
}
