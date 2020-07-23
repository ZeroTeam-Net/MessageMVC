using Agebull.Common.Logging;
using Confluent.Kafka;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageQueue;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     Kafka消息发布
    /// </summary>
    internal sealed class KafkaPoster : BackgroundPoster<QueueItem>
    {
        #region IFlowMiddleware 

        private static IProducer<Null, string> Producer => KafkaFlow.Instance.producer;

        internal KafkaPoster()
        {
            Name = nameof(KafkaPoster);
        }

        /// <summary>
        /// 关闭处理
        /// </summary>
        protected override Task OnClose()
        {
            cancellation.Dispose();
            cancellation = null;
            State = StationStateType.Closed;
            return Task.CompletedTask;
        }

        protected override async Task<bool> DoPost(QueueItem item)
        {
            Logger.Trace(() => $"[异步消息投递] 正在投递消息.{KafkaOption.Instance.BootstrapServers}");
            var ret = await Producer.ProduceAsync(item.Topic, new Message<Null, string>
            {
                Value = item.Message
            });
            Logger.Trace(() => $"[异步消息投递] 投递结果:{ret.ToJson()}");
            return ret.Status != PersistenceStatus.NotPersisted;
        }
        #endregion

    }
}

