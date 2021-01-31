using Agebull.Common.Logging;
using BeetleX;
using BeetleX.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tcp
{

    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class TcpPoster : BackgroundPoster<QueueItem>, IMessagePoster, IFlowMiddleware
    {
        #region 基本

        /// <summary>
        /// 构造
        /// </summary>
        public TcpPoster()
        {
            Name = nameof(Name);
            AsyncPost = true;
        }

        /// <summary>
        /// 单例
        /// </summary>
        public static TcpPoster Instance = new TcpPoster();


        int IZeroMiddleware.Level => MiddlewareLevel.General;

        string IZeroDependency.Name => nameof(TcpPoster);

        bool isConnect;

        AsyncTcpClient client;
        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            if (TcpOption.Instance.Client == null || TcpOption.Instance.Client.Address.IsNullOrEmpty() || TcpOption.Instance.Client.Port <= 1024 || TcpOption.Instance.Client.Port >= short.MaxValue)
                return Task.CompletedTask;
            client = SocketFactory.CreateClient<AsyncTcpClient>(TcpOption.Instance.Client.Address, TcpOption.Instance.Client.Port);
            client.DataReceive = EventClientReceive;
            isConnect = client.Connect(out _);
            _ = CheckTimeOut();
            return Task.CompletedTask;
        }

        Task ILifeFlow.Destory()
        {
            client?.DisConnect();
            client?.Dispose();
            return Task.CompletedTask;
        }
        #endregion

        #region 日志

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="log"></param>
        protected override void RecordLog(LogLevel level, string log)
        {
            Console.WriteLine(log);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="log"></param>
        protected override void RecordLog(LogLevel level, Func<string> log)
        {
            Console.WriteLine(log());
        }
        #endregion

        #region 等待队列

        class PostTask
        {
            public DateTime Start { get; set; }
            public TaskCompletionSource<IMessageResult> Task { get; set; }
        }
        readonly ConcurrentDictionary<string, PostTask> waiting = new ConcurrentDictionary<string, PostTask>();

        /// <summary>
        /// 初始化
        /// </summary>
        async Task CheckTimeOut()
        {
            while (State <= StationStateType.Stop)
            {
                try
                {
                    if (waiting.Count == 0)
                    {
                        await Task.Delay(3000);
                        continue;
                    }
                    else
                        await Task.Delay(1000);
                    var items = waiting.ToArray();
                    foreach (var item in items)
                    {
                        var len = (DateTime.Now - item.Value.Start).TotalSeconds;
                        if (len > 3)
                            item.Value.Task.TrySetException(new TimeoutException());
                        if (len > 10)
                            waiting.TryRemove(item.Key, out _);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        #endregion

        #region 接收与发送

        void EventClientReceive(IClient client, ClientReceiveArgs reader)
        {
            try
            {
                var pipestream = reader.Stream.ToPipeStream();
                if (pipestream.TryReadLine(out string msg) && SmartSerializer.TryToResult(msg, out var message) && waiting.TryRemove(message.ID, out var item))
                {
                    item.Task.TrySetResult(message);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }
        /// <inheritdoc/>
        protected override async Task<bool> DoPost(QueueItem item)
        {
            if (client == null)
            {
                return false;
            }
            if (!isConnect || !client.IsConnected)
                isConnect = client.Connect(out _);
            if (!isConnect || !client.IsConnected)
                return false;
            client.Send(p => p.WriteLine(SmartSerializer.ToInnerString(item.Message)));
            if (!client.IsConnected)
            {
                return false;
            }
            var task = new TaskCompletionSource<IMessageResult>();
            waiting.TryAdd(item.ID, new PostTask
            {
                Task = task,
                Start = DateTime.Now
            });
            try
            {
                var resut = await task.Task;
                return resut.State.IsEnd();
            }
            catch (Exception ex)
            {
                RecordLog(LogLevel.Error, ex.ToString());
                return false;
            }
        }
        #endregion
    }
}