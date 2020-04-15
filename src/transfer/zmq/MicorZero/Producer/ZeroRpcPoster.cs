using Agebull.Common.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     ZMQ生产者
    /// </summary>
    public class ZeroRPCPoster : JsonSerializeProxy, IMessagePoster
    {
        #region Properties

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static ZeroRPCPoster Instance = new ZeroRPCPoster();

        /// <summary>
        /// 构造
        /// </summary>
        public ZeroRPCPoster()
        {
            Instance = this;
        }

        #endregion

        #region IMessagePoster

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task<IMessageResult> Post(IInlineMessage message)
        {
            LogRecorder.MonitorTrace("[ZeroRPCPoster.Post] 开始发送");
            return await new ZeroCaller
            {
                Message = message
            }.CallAsync();
        }

        #endregion
    }
}

/*

        string IMessagePoster.Producer(string topic, string title, string content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
            return client.Result;
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
        void IMessagePoster.Producer<TArg>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
        }
        TRes IMessagePoster.Producer<TRes>(string topic, string title)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title
            };
            client.CallCommand();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }


        async Task<string> IMessagePoster.ProducerAsync(string topic, string title, string content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            await client.CallCommandAsync();
            return client.Result;
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            await client.CallCommandAsync();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
        Task IMessagePoster.ProducerAsync<TArg>(string topic, string title, TArg content)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            return client.CallCommandAsync();
        }
        async Task<TRes> IMessagePoster.ProducerAsync<TRes>(string topic, string title)
        {
            var client = new ZeroRPCProducer
            {
                Station = topic,
                Commmand = title
            };
            await client.CallCommandAsync();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
*/
