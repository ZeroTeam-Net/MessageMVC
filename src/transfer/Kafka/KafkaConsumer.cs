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
                    var cr = consumer.Consume(token);
                    if (cr == null || cr.IsPartitionEOF)
                    {
                        continue;
                    }
                    try
                    {
                        await OnMessagePush(cr);

                    }
                    catch (Exception e)
                    {
                        Logger.Exception(e);
                        consumer.Commit();//无法处理的消息,直接确认
                        continue;
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

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="consumeResult"></param>
        private async Task OnMessagePush(ConsumeResult consumeResult)
        {
            if (SmartSerializer.TryToMessage(consumeResult.Message.Value, out var message))
                await MessageProcessor.OnMessagePush(Service, message, true, consumeResult);//BUG:应该配置化同步或异步
        }

        private ConsumerBuilder<Ignore, string> builder;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            builder = new ConsumerBuilder<Ignore, string>(KafkaOption.Instance.Consumer);
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
        Task<bool> IMessageReceiver.OnResult(IInlineMessage item, object tag)
        {
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
    }
}
