using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Kafka
{
    internal class KafkaConsumer : IMessageConsumer
    {
        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, WaitCount, ErrorCount, SuccessCount;//, RecvCount, SendCount, SendError;


        /// <summary>
        /// 初始化
        /// </summary>
        void INetTransfer.Initialize()
        {
            config = ConfigurationManager.Get<ConsumerConfig>("Kafka");
        }

        private IConsumer<Ignore, string> consumer;
        private ConsumerConfig config;


        public ZeroService Station { get; set; }

        public string Name { get; set; }

        IService INetTransfer.Service { get => Station; set => Station = value as ZeroService; }

        async Task<bool> INetTransfer.Loop(CancellationToken token)
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
                        await OnMessagePush(item);
                        consumer.Commit();
                    }
                    catch (Exception e)
                    {
                        await ((IMessageConsumer)this).OnMessageError(e, new MessageItem
                        {
                            State = MessageState.FormalError,
                        }, cr.Value);
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
        private async Task OnMessagePush(IMessageItem message)
        {
            var state = await MessageProcess.OnMessagePush(Station, message);
            if (state == MessageState.Success)
            {
                Interlocked.Increment(ref SuccessCount);
            }
            else
            {
                Interlocked.Increment(ref ErrorCount);
            }

            Interlocked.Decrement(ref WaitCount);
        }

        /// <summary>
        /// 将要开始
        /// </summary>
        Task<bool> INetTransfer.Prepare()
        {
            return Task.FromResult(true);
        }

        private ConsumerBuilder<Ignore, string> builder;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> INetTransfer.LoopBegin()
        {
            builder = new ConsumerBuilder<Ignore, string>(config);
            consumer = builder.Build();
            consumer.Subscribe(Station.ServiceName);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.LoopComplete()
        {
            consumer.Close();
            consumer.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 表示已成功接收 
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.Commit()
        {
            consumer.Commit();
            return Task.CompletedTask;
        }

    }
}
