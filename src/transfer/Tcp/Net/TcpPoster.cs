using Agebull.Common.Ioc;
using BeetleX;
using BeetleX.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageQueue;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.Tcp
{

    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class TcpPoster : BackgroundPoster<QueueItem>, ILifeFlow, IZeroDiscover
    {
        #region 基本

        /// <summary>
        /// 征集周期管理器
        /// </summary>
        protected override ILifeFlow LifeFlow => this;

        /// <summary>
        /// 构造
        /// </summary>
        public TcpPoster()
        {
            Name = nameof(TcpPoster);
            AsyncPost = true;
        }

        /// <summary>
        /// 单例
        /// </summary>
        public static TcpPoster Instance = new TcpPoster();

        // bool isConnect;

        AsyncTcpClient client;
        /// <summary>
        /// 检查期间就开启服务
        /// </summary>
        Task IZeroDiscover.Discovery()
        {
            if (TcpOption.Instance.Client == null || TcpOption.Instance.Client.Address.IsMissing() || 
                TcpOption.Instance.Client.Port <= 1024 || TcpOption.Instance.Client.Port >= short.MaxValue)
                return Task.CompletedTask;
            client = SocketFactory.CreateClient<AsyncTcpClient>(TcpOption.Instance.Client.Address, TcpOption.Instance.Client.Port);
            client.DataReceive = EventClientReceive;
            client.ClientError = EventClientError;
            client.Disconnected = Disconnected;
            client.Connected = Connected;
            client.Connect(out _);
            DoStart();
            RecordLog(LogLevel.Information, $"{Name}已开启");
            return Task.CompletedTask;
        }

        async Task ILifeFlow.Destroy()
        {
            await Destroy();
            //client?.DisConnect();
            //client.Socket.Disconnect(false);
            //client.Socket.Dispose();
            client?.Dispose();
            RecordLog(LogLevel.Information, $"{Name}已关闭");
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
            if (!TcpOption.Instance.Client.IsLog)
            {
                base.RecordLog(level, log);
                return;
            }
            if (!Logger.IsEnabled(level))
                return;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{level}] {Name} {DateTime.Now} \n\t{log}");
            Console.ResetColor();
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="log"></param>
        protected override void RecordLog(LogLevel level, Func<string> log)
        {
            if (!TcpOption.Instance.Client.IsLog)
            {
                base.RecordLog(level, log);
                return;
            }
            if (!Logger.IsEnabled(level))
                return;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{level}] {Name} {DateTime.Now} \n\t{log()}");
            Console.ResetColor();
        }
        #endregion

        #region 接收与发送
        void Connected(IClient c)
        {
            RecordLog(LogLevel.Information, "链接成功");
        }
        void Disconnected(IClient c)
        {
            RecordLog(LogLevel.Information, "链接关闭");
        }
        void EventClientError(IClient c, ClientErrorArgs e)
        {
            if (CurrentTask == null)
            {
                RecordLog(LogLevel.Error, $"[NoMessage]{e.Message}\n{e.Error}");
                return;
            }
            var task = CurrentTask;
            RecordLog(LogLevel.Error, $"[{task.Message.ID}]{e.Message}\n{e.Error}");
            task.Message.State = MessageState.NetworkError;
            task.Result.State = MessageState.NetworkError;
            var wait = task.WaitingTask;
            if (wait != null)
                wait.TrySetResult(task.Result);
        }
        void EventClientReceive(IClient client, ClientReceiveArgs reader)
        {
            try
            {
                var pipeStream = reader.Stream.ToPipeStream();
                if (!pipeStream.TryReadLine(out var msg) || msg == null)
                {
                    return;
                }
                msg = msg.Trim('\0').Trim();
                if (msg.IsMissing())
                {
                    return;
                }
                IInlineMessage message;
                if (msg[0] == '#')
                {
                    message = new InlineMessage
                    {
                        ID = msg[1..],
                        State = MessageState.Success
                    };
                }
                else if (msg[0] == '@')
                {
                    var words = msg[1..].Split(new char[] { '@', '#' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    message = new InlineMessage
                    {
                        ID = msg,
                        Service = TcpApp.ClientOptionService,
                        Method = words[0],
                        Argument = words[1],
                        State = MessageState.ServerMessage
                    };
                }
                else if (!SmartSerializer.TryToMessage(msg, out message))
                    return;
                if (message.State == MessageState.ServerMessage)
                {
                    if (ZeroAppOption.Instance.IsRuning)
                    {
                        var service = ZeroFlowControl.GetService(message.Service) ?? new ZeroService
                        {
                            ServiceName = message.Service,
                            Receiver = new EmptyReceiver(),
                            Serialize = DependencyHelper.GetService<ISerializeProxy>()
                        };

                        MessageProcessor.RunOnMessagePush(service, message, false, new TcpWriter());
                    }
                    return;
                }
                else if (PostTasks.TryRemove(message.ID, out var item))
                {
                    item.Result.State = message.State;
                    item.Result.Trace = message.TraceInfo;
                    item.Result.Result = message.Result;
                    item.Result.ResultData = message.ResultData;
                    if (item.WaitingTask != null)
                        item.WaitingTask.TrySetResult(item.Result);
                    else
                        item.Message.State = message.State;
                }
            }
            catch (Exception ex)
            {
                RecordLog(LogLevel.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 执行发送
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override TaskCompletionSource<IMessageResult> DoPost(QueueItem item)
        {
            RecordLog(LogLevel.Trace, () => $"[异步消息投递] {item.ID} 正在投递消息({TcpOption.Instance.Client.Address}:{TcpOption.Instance.Client.Port})");
            var res = new TaskCompletionSource<IMessageResult>();
            if (client == null)
            {
                RecordLog(LogLevel.Error, () => $"[异步消息投递] 服务未连接");

                res.TrySetResult(new MessageResult
                {
                    ID = item.Message.ID,
                    Trace = item.Message.TraceInfo,
                    State = MessageState.Cancel
                });
                return res;
            }
            //if (!isConnect || !client.IsConnected)
            //    isConnect = client.Connect(out _);
            //if (!isConnect || !client.IsConnected)
            //{
            //    Logger.Error(() => $"[异步消息投递] 服务未连接");

            //    res.TrySetResult(new MessageResult
            //    {
            //        ID = item.Message.ID,
            //        Trace = item.Message.TraceInfo,
            //        State = MessageState.Cancel
            //    });
            //    return res;
            //}
            client.Send(p => p.WriteLine(SmartSerializer.ToInnerString(item.Message)));

            return res;
        }

        #endregion
    }
}