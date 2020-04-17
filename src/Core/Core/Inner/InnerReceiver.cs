using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///    内部生产者
    /// </summary>
    public class InnerReceiver : MessageReceiverBase, IServiceReceiver, IMessageConsumer, INetEvent
    {
        /// <summary>
        /// 构造
        /// </summary>
        public InnerReceiver() : base(nameof(InnerReceiver))
        {
        }

        private TaskCompletionSource<bool> task;
        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            task = new TaskCompletionSource<bool>();
            return task.Task;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.Close()
        {
            task.SetResult(true);
            return Task.CompletedTask;
        }
    }
}
/*
        
        private string DoProducer(string channel, string title, string content)
        {
            var item = new RedisQueueItem
            {
                ID = Guid.NewGuid().ToString("N"),
                Channel = channel,
                Message = JsonHelper.SerializeObject(new MessageItem
                {
                    Title = title,
                    Content = content
                })
            };
            redisQueues.Enqueue(item);
            semaphore.Release();
            return item.ID;
        }


        /// <inheritdoc/>
        public string Producer(string channel, string title, string content)
        {
            return DoProducer(channel, title, content);
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, JsonHelper.SerializeObject(content));
            return default;
        }

        /// <inheritdoc/>
        public void Producer<TArg>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, JsonHelper.SerializeObject(content));
        }
        TRes IMessagePoster.Producer<TRes>(string channel, string title)
        {
            DoProducer(channel, title, null);
            return default;
        }


        Task<string> IMessagePoster.ProducerAsync(string channel, string title, string content)
        {
            var id = DoProducer(channel, title, null);
            return Task.FromResult(id);
        }

        Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, JsonHelper.SerializeObject(content));
            return Task.FromResult(default(TRes));
        }
        Task IMessagePoster.ProducerAsync<TArg>(string channel, string title, TArg content)
        {
            DoProducer(channel, title, null);
            return Task.CompletedTask;
        }

        Task<TRes> IMessagePoster.ProducerAsync<TRes>(string channel, string title)
        {
            DoProducer(channel, title, null);
            return Task.FromResult(default(TRes));
        }

    */
