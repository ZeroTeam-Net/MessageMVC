using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     API客户端代理
    /// </summary>
    public class ZeroPostProxy : IFlowMiddleware
    {
        #region IFlowMiddleware 

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(ZeroPostProxy);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Front;

        ILogger logger;

        /// <summary>
        /// 初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            logger.Information("ZeroPostProxy >>> Initialize");
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ZeroPostProxy));
            ZeroRpcFlow.ZeroNetEvents.Add(OnZeroNetEvent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 等待数量
        /// </summary>
        public int WaitCount;

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ZeroPostProxy Instance = new ZeroPostProxy();


        #endregion

        #region 站点变更

        /// <summary>
        /// 站点是否已修改
        /// </summary>
        private static bool _isChanged;

        /// <summary>
        /// 所有代理
        /// </summary>
        internal static readonly ConcurrentDictionary<string, StationProxyItem> StationProxy = new ConcurrentDictionary<string, StationProxyItem>(StringComparer.OrdinalIgnoreCase);

        private Task OnZeroNetEvent(ZeroRpcOption config, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.CenterSystemStop:
                    StationProxy.Clear();
                    _isChanged = true;
                    return Task.CompletedTask;
                case ZeroNetEventType.CenterSystemStart:
                    SyncStationConfig();
                    return Task.CompletedTask;
                case ZeroNetEventType.ConfigUpdate:
                case ZeroNetEventType.CenterStationJoin:
                case ZeroNetEventType.CenterStationInstall:
                case ZeroNetEventType.CenterStationUpdate:
                    if (e.EventConfig?.StationName != null &&
                        StationProxy.TryAdd(e.EventConfig.StationName, new StationProxyItem
                        {
                            Config = e.EventConfig
                        }))
                    {
                        _isChanged = true;
                    }
                    break;
                //case ZeroNetEventType.CenterStationResume:
                //case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                case ZeroNetEventType.CenterStationStop:
                    if (e.EventConfig?.StationName != null && StationProxy.TryGetValue(e.EventConfig.StationName, out var item))
                    {
                        item.Config.State = ZeroCenterState.Stop;
                        _isChanged = true;
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        private static void Reload()
        {
            StationProxy.Clear();
            foreach (var config in ZeroRpcFlow.Config.GetConfigs())
            {
                StationProxy.TryAdd(config.StationName, new StationProxyItem
                {
                    Config = config
                });
            }
            _isChanged = true;
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 启动
        /// </summary>
        Task ILifeFlow.Open()
        {
            logger.Information("ZeroPostProxy >>> 启动");
            return Task.Factory.StartNew(Loop);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public bool Loop()
        {
            logger.Information("Run");
            SyncStationConfig();
            _proxyServiceSocket = ZSocketEx.CreateServiceSocket(InprocAddress, null, ZSocketType.ROUTER);
            ZeroRPCPoster.Instance.State = StationStateType.Run;
            while (CanLoopEx)
            {
                Listen();
            }
            ZeroRPCPoster.Instance.State = StationStateType.Closed;
            logger.Information("Close");
            return true;
        }

        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        Task ILifeFlow.Destory()
        {
            logger.Information("ZeroPostProxy >>> Check");
            _proxyServiceSocket.Dispose();
            _zmqPool?.Dispose();

            foreach (var proxyItem in StationProxy.Values)
            {
                proxyItem.Socket?.Dispose();
            }
            StationProxy.Clear();
            return Task.CompletedTask;
        }

        #endregion

        #region 请求


        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<IMessageResult> CallZero(IInlineMessage argument, byte[] description, params byte[][] args)
        {
            var info = new ZeroRPCTaskInfo
            {
                Argument = new ZeroArgument
                {
                    Message = argument
                }
            };


            var socket = CreateProxySocket(info.Argument);
            if (socket == null)
            {
                LogRecorder.MonitorInfomation(() => $"[ZeroPostProxy.CallZero] 本地Sock构造失败.{argument.ID}");
                return Task.FromResult<IMessageResult>(null);
            }
            using var zMessage = new ZMessage
                {
                    new ZFrame(argument.ServiceName),
                    new ZFrame(description)
                };
            if (args != null)
            {
                foreach (var arg in args)
                {
                    zMessage.Add(new ZFrame(arg));
                }
                zMessage.Add(new ZFrame(info.Argument.Config.ServiceKey));
            }

            info.TaskSource = new TaskCompletionSource<IMessageResult>();
            ZeroRPCTaskInfo.Tasks.TryAdd(argument.ID, info);
            bool res;
            using (socket)
            {
                res = socket.Send(zMessage, out ZError _);
            }
            if (!res)
            {
                LogRecorder.MonitorInfomation(() => $"[ZeroPostProxy.CallZero] 发送到队列失败.{InprocAddress}");
                argument.RealState = MessageState.NetworkError;
                info.TaskSource.TrySetResult(null);
            }
            else
            {
                LogRecorder.MonitorDetails(() => $"[ZeroPostProxy.CallZero] 已发送到异步队列");
                argument.RealState = MessageState.Send;
            }
            return info.TaskSource.Task;
        }

        private ZSocketEx CreateProxySocket(ZeroArgument argument)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(argument.Message.ServiceName, out argument.Config))
            {
                argument.Message.RealState = MessageState.Unhandled;
                return null;
            }

            if (argument.Config.State != ZeroCenterState.None && argument.Config.State != ZeroCenterState.Run)
            {
                argument.Message.RealState = MessageState.Unhandled;
                return null;
            }
            return ZSocketEx.CreateOnceSocket(InprocAddress, null, argument.Message.ID.ToBytes(), ZSocketType.PAIR);
        }
        #endregion

        #region ZMQ Socket

        /// <summary>
        /// Pool对象
        /// </summary>
        private IZmqPool _zmqPool;

        /// <summary>
        /// 代理地址
        /// </summary>
        private const string InprocAddress = "inproc://ZeroRPCProxy.req";

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket _proxyServiceSocket;

        /// <summary>
        /// 构造Pool
        /// </summary>
        public IZmqPool CreatePool()
        {
            var sockets = _zmqPool == null
                 ? new List<ZSocket>()
                  : _zmqPool.Sockets.ToList();
            _isChanged = false;
            if (_zmqPool != null)
            {
                _zmqPool.Sockets = null;
                _zmqPool.Dispose();
            }
            //var added = StationProxy.Values.Where(p => p.Config.State > ZeroCenterState.Pause).Select(p => p.Socket).ToArray();
            var alive = StationProxy.Values.ToArray();
            var list = new List<ZSocket>
            {
                _proxyServiceSocket
            };
            foreach (var item in alive)
            {
                if (item.Config.State != ZeroCenterState.None && item.Config.State != ZeroCenterState.Run)
                {
                    StationProxy.TryRemove(item.Config.StationName, out _);
                    if (!sockets.Contains(item.Socket))
                        sockets.Add(item.Socket);
                    continue;
                }
                if (item.Socket == null)
                {
                    item.Socket = ZSocketEx.CreatePoolSocket(item.Config.RequestAddress,
                        item.Config.ServiceKey,
                        ZSocketType.DEALER,
                        ZSocketHelper.CreateIdentity(false, item.Config.Name));
                    item.Open = DateTime.Now;
                }
                else
                {
                    sockets.Remove(item.Socket);
                }
                list.Add(item.Socket);
            }
            //防止丢失
            sockets.ForEach(p => p.Dispose());

            _zmqPool = ZmqPool.CreateZmqPool();
            _zmqPool.Sockets = list.ToArray();
            _zmqPool.RePrepare(ZPollEvent.In);
            return _zmqPool;
        }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        protected bool CanLoopEx => WaitCount > 0 || ZeroFlowControl.IsRuning;
        /// <summary>
        /// 同步站点配置
        /// </summary>
        private void SyncStationConfig()
        {
            logger.Information("同步站点配置");
            foreach (var config in ZeroRpcFlow.Config.GetConfigs())
            {
                StationProxy.TryAdd(config.StationName, new StationProxyItem
                {
                    Config = config
                });
            }
            _isChanged = true;
        }
        private void Listen()
        {
            try
            {
                if (_isChanged)
                {
                    _zmqPool = CreatePool();
                }
                if (!_zmqPool.Poll())
                {
                    CheckTimeOut();
                    return;
                }

                if (_zmqPool.CheckIn(0, out var message))
                {
                    OnLocalCall(message);
                }

                for (int idx = 1; idx < _zmqPool.Size; idx++)
                {
                    if (_zmqPool.CheckIn(idx, out message))
                    {
                        OnRemoteResult(message);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Information("Listen exception {0}", e.Message);
            }
        }

        #endregion

        #region 超时检测

        private void CheckTimeOut()
        {
            int timeout = ZeroRpcFlow.Config.StationOption.ApiTimeout < 500
                ? 500
                : ZeroRpcFlow.Config.StationOption.ApiTimeout;
            var tasks = ZeroRPCTaskInfo.Tasks.Values.ToArray();
            foreach (var task in tasks)
            {
                if ((DateTime.Now - task.Start).TotalSeconds >= timeout)
                {
                    logger.Information(() => $"[ZeroPostProxy.CheckTimeOut] 超时.{task.Argument.Message.ID}");

                    try
                    {
                        task.Argument.Message.State = MessageState.NetworkError;
                        task.TaskSource.TrySetResult(null);
                    }
                    catch
                    {
                    }
                    ZeroRPCTaskInfo.Tasks.Remove(task.Argument.Message.ID, out _);
                }
            }
        }
        #endregion

        #region ZMQ消息处理

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="message"></param>
        private void OnLocalCall(ZMessage message)
        {
            string id;
            ZMessage message2;
            StationProxyItem item;
            ZeroRPCTaskInfo task;
            using (message)
            {
                id = message[0].ToString();
                if (!ZeroRPCTaskInfo.Tasks.TryGetValue(id, out task))
                {
                    logger.Error("({0})消息格式错乱", id);
                    return;
                }
                var station = message[1].ToString();
                if (!StationProxy.TryGetValue(station, out item))
                {
                    logger.Error("({0})站点[{1}]不存在.", id, station);
                    task.Argument.Message.RealState = MessageState.Unhandled;
                    task.TaskSource.SetResult(null);//直接使用状态
                    ZeroRPCTaskInfo.Tasks.TryRemove(id, out _);
                    return;
                }
                message2 = message.Duplicate(2);
            }
            logger.Trace("({0})正在发送到远程服务", id);
            bool success;
            ZError error;
            using (message2)
            {
                success = item.Socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
            }

            if (success)
            {
                logger.Trace("({0})发送到远程服务成功,等待返回", id);
                task.Argument.Message.RealState = MessageState.Recive;
                Interlocked.Increment(ref WaitCount);
            }
            else
            {
                logger.Trace("({0})发送到远程服务失败.{1}", id, error.Text);
                task.Argument.Message.RealState = MessageState.NetworkError;
                task.TaskSource.SetResult(null);//直接使用状态
                ZeroRPCTaskInfo.Tasks.TryRemove(id, out _);
            }
        }

        private void OnRemoteResult(ZMessage message)
        {
            string json = null;
            ZeroResult zeroResult = ZeroResultData.Unpack<ZeroResult>(message, true, (res, type, bytes) =>
            {
                switch (type)
                {
                    case ZeroFrameType.ResultText:
                        res.Result = ZeroNetMessage.GetString(bytes);
                        return true;
                    case ZeroFrameType.ExtendText:
                        json = ZeroNetMessage.GetString(bytes);
                        return true;
                        //case ZeroFrameType.BinaryContent:
                        //    res.Binary = bytes;
                        //    return true;
                }
                return false;
            });

            if (!ZeroRPCTaskInfo.Tasks.TryGetValue(zeroResult.Requester, out var task))
            {
                logger.Error("({0})接收到远程返回,原始请求无法还原.{1}", zeroResult.Requester, zeroResult.ToString());
                return;
            }
            if (zeroResult.State == ZeroOperatorStateType.Runing)
            {
                task.Argument.Message.RealState = MessageState.Recive;
                logger.Trace("({0}).远程通知正在处理中");
                return;
            }

            Interlocked.Decrement(ref WaitCount);
            ZeroRPCTaskInfo.Tasks.TryRemove(zeroResult.Requester, out _);


            task.Argument.Message.Result = zeroResult.Result;
            IMessageResult messageResult;
            if (SmartSerializer.TryFromInnerString<MessageResult>(json, out var result))
            {
                messageResult = result;
                task.Argument.Message.RealState = result.State;
                task.Argument.Message.Trace = result.Trace;
            }
            else
            {
                task.Argument.Message.RealState = ZeroRPCPoster.GetMessageState(zeroResult.State);
                messageResult = task.Argument.Message.ToStateResult();
            }
            messageResult.DataState |= MessageDataState.ResultOffline;
            messageResult.DataState &= ~MessageDataState.ResultInline;
            logger.Trace("({0})接收到远程返回.{1} => {2}", zeroResult.Requester, messageResult.State, messageResult.Result);
            try
            {
                if (!task.TaskSource.TrySetResult(messageResult))
                {
                    logger.Trace("({0})接收到远程返回.但等待队列已注销:", zeroResult.Requester);
                }
            }
            catch
            {
                logger.Trace("({0})接收到远程返回.但等待队列已注销:", zeroResult.Requester);
            }
        }

        #endregion
    }
}

