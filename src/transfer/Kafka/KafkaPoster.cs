using Agebull.Common.Logging;
using Confluent.Kafka;
using System.Text.Json;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Kafka
{
    /// <summary>
    ///     Kafka消息发布
    /// </summary>
    internal sealed class KafkaPoster : BackgroundPoster<QueueItem>
    {
        public KafkaPoster()
        {
            Name = nameof(KafkaPoster);
            AsyncPost = KafkaOption.Instance.Message.AsyncPost;
        }

        protected override async Task<bool> DoPost(QueueItem item)
        {
            Logger.Trace(() => $"[异步消息投递] {item.ID} 正在投递消息.{KafkaOption.Instance.BootstrapServers}");
            var ret = await KafkaFlow.Instance.producer.ProduceAsync(item.Topic, new Message<byte[], string>
            {
                Key = JsonSerializer.SerializeToUtf8Bytes(new MessageItem
                {
                    ID = item.Message.ID,
                    Method = item.Message.Method,
                    TraceInfo = item.Message.TraceInfo,
                    Context = item.Message.Context,
                    User = item.Message.User,
                }),
                Value = item.Message.Argument,
                //Headers = headers
            });
            Logger.Trace(() => $"[异步消息投递] {item.ID} 投递结果:{ret.ToJson()}");
            return ret.Status != PersistenceStatus.NotPersisted;
        }
    }
}
/*// <summary>
/// 
/// </summary>
internal class KafkaKey
{
    /// <summary>
    /// 消息标识
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// 方法
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    ///     跟踪信息
    /// </summary>
    public TraceInfo Trace { get; set; }

    /// <summary>
    /// 上下文信息
    /// </summary>
    public Dictionary<string, string> Context { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public Dictionary<string, string> User { get; set; }

}

        //var headers = new Headers
        //{
        //    { "zid", Encoding.ASCII.GetBytes(item.Message.ID) },
        //    { "method", Encoding.ASCII.GetBytes(item.Message.Method) }
        //};
        //if (item.Message.Trace != null)
        //    headers.Add("trace", JsonSerializer.SerializeToUtf8Bytes(item.Message.Trace));
        //if (item.Message.Context != null)
        //    headers.Add("ctx", JsonSerializer.SerializeToUtf8Bytes(item.Message.Context));
        //if (item.Message.User != null)
        //    headers.Add("ctx", JsonSerializer.SerializeToUtf8Bytes(item.Message.User));
*/

