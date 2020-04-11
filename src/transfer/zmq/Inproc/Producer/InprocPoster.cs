using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    ///     ZMQ生产者
    /// </summary>
    public class InprocPoster : NewtonJsonSerializeProxy, IMessagePoster
    {
        #region Properties

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static InprocPoster Instance = new InprocPoster();

        /// <summary>
        /// 简单调用
        /// </summary>
        /// <remarks>
        /// 1 不获取全局标识(过时）
        /// 2 无远程定向路由,
        /// 3 无上下文信息
        /// </remarks>
        public bool Simple { set; get; }

        #endregion

        #region IMessagePoster

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task<IInlineMessage> Post(IMessageItem message)
        {
            var client = new ZmqCaller
            {
                Message = message.ToInline()
            };
            return await client.CallAsync();
        }
        #endregion

    }
}
/*

        string IMessagePoster.Producer(string topic, string title, string content)
        {
            var client = new InporcProducer
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
            var client = new InporcProducer
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
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
        }
        TRes IMessagePoster.Producer<TRes>(string topic, string title)
        {
            var client = new InporcProducer
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
            var client = new InporcProducer
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
            var client = new InporcProducer
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
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            return client.CallCommandAsync();
        }
        async Task<TRes> IMessagePoster.ProducerAsync<TRes>(string topic, string title)
        {
            var client = new InporcProducer
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
