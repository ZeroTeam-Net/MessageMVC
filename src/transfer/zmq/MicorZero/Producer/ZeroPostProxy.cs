using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
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
        int IZeroMiddleware.Level => 0;

        ILogger logger;

        /// <summary>
        /// 初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ZeroPostProxy));
            ZeroRpcFlow.ZeroNetEvents.Add(OnZeroNetEvent);
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
        internal static readonly Dictionary<string, StationProxyItem> StationProxy = new Dictionary<string, StationProxyItem>(StringComparer.OrdinalIgnoreCase);

        private Task OnZeroNetEvent(ZeroRpcOption config, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
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
                case ZeroNetEventType.CenterStationResume:
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                case ZeroNetEventType.CenterStationStop:
                    _isChanged = true;
                    break;
            }
            return Task.CompletedTask;
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 构造
        /// </summary>
        void IFlowMiddleware.Start()
        {
            Task.Run(Loop);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public bool Loop()
        {
            Prepare();

            logger.Information("Run");
            ((IMessagePoster)ZeroRPCPoster.Instance).State = StationStateType.Run;
            while (CanLoopEx)
            {
                Listen();
            }
            logger.Information("Close");
            ((IMessagePoster)ZeroRPCPoster.Instance).State = StationStateType.Closed;
            return true;
        }

        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        void IFlowMiddleware.End()
        {
            logger.Information("End");
            _proxyServiceSocket.Dispose();
            _zmqPool?.Dispose();

            foreach (var proxyItem in StationProxy.Values)
            {
                proxyItem.Socket?.Dispose();
            }
            StationProxy.Clear();
        }

        #endregion

        #region 请求


        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<MessageResult> CallZero(ZeroCaller caller, byte[] description, params byte[][] args)
        {
            var info = new ZeroRPCTaskInfo
            {
                Caller = caller
            };


            var socket = CreateProxySocket(caller);
            if (socket == null)
            {
                LogRecorder.MonitorTrace(() => $"[ZeroPostProxy.CallZero] 本地Sock构造失败.{caller.Message.State}");
                return Task.FromResult(new MessageResult
                {
                    ID = caller.Message.ID,
                    State = MessageState.FormalError,
                    Trace = caller.Message.Trace,
                    RuntimeStatus = ApiResultHelper.Helper.ArgumentError
                });
            }
            using var message = new ZMessage
                {
                    new ZFrame(caller.Message.ServiceName),
                    new ZFrame(description)
                };
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message.Add(new ZFrame(arg));
                }
                message.Add(new ZFrame(caller.Config.ServiceKey));
            }

            bool res;
            using (socket)
            {
                res = socket.Send(message, out ZError _);
            }
            if (!res)
            {
                LogRecorder.MonitorTrace(() => $"[ZeroPostProxy.CallZero] 发送到队列失败.{InprocAddress}");
                return Task.FromResult(new MessageResult
                {
                    ID = caller.Message.ID,
                    Trace = caller.Message.Trace,
                    State = MessageState.NetworkError,
                    RuntimeStatus = ApiResultHelper.Helper.NetworkError
                });
            }
            LogRecorder.MonitorTrace(() => $"[ZeroPostProxy.CallZero] 已发送到异步队列");

            ZeroRPCTaskInfo.Tasks.TryAdd(caller.Message.ID, info);
            info.TaskSource = new TaskCompletionSource<MessageResult>();
            return info.TaskSource.Task;
        }

        private ZSocketEx CreateProxySocket(ZeroCaller caller)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(caller.Message.ServiceName, out caller.Config))
            {
                caller.Message.RuntimeStatus = ApiResultHelper.Helper.NoFind;
                caller.Message.State = MessageState.NonSupport;
                return null;
            }

            if (caller.Config.State != ZeroCenterState.None && caller.Config.State != ZeroCenterState.Run)
            {
                caller.Message.RuntimeStatus = caller.Config.State == ZeroCenterState.Pause
                    ? ApiResultHelper.Helper.Pause
                    : ApiResultHelper.Helper.NonSupport;
                caller.Message.State = MessageState.NonSupport;
                return null;
            }

            caller.Message.State = MessageState.Accept;

            return ZSocketEx.CreateOnceSocket(InprocAddress, null, caller.Message.ID.ToBytes(), ZSocketType.PAIR);
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
                if (item.Config.State == ZeroCenterState.None || item.Config.State == ZeroCenterState.Run)
                {
                    if (item.Socket == null)
                    {
                        item.Socket = ZSocketEx.CreatePoolSocket(item.Config.RequestAddress,
                            item.Config.ServiceKey,
                            ZSocketType.DEALER,
                            ZSocketHelper.CreateIdentity(false, item.Config.Name));
                        item.Open = DateTime.Now;
                    }
                    list.Add(item.Socket);
                }
                else
                {
                    item.Socket?.Dispose();
                }
            }
            _zmqPool = ZmqPool.CreateZmqPool();
            _zmqPool.Sockets = list.ToArray();
            _zmqPool.RePrepare(ZPollEvent.In);
            return _zmqPool;
        }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        protected bool CanLoopEx => WaitCount > 0 || ZeroFlowControl.IsRuning;

        private void Prepare()
        {
            logger.Information("Prepare");
            var identity = ZeroAppOption.Instance.TraceName.ToBytes();
            foreach (var config in ZeroRpcFlow.Config.GetConfigs())
            {
                StationProxy.Add(config.StationName, new StationProxyItem
                {
                    Config = config,
                    Open = DateTime.Now,
                    Socket = ZSocketEx.CreateLongLink(config.RequestAddress, config.ServiceKey, ZSocketType.DEALER, identity)
                });
            }
            _proxyServiceSocket = ZSocketEx.CreateServiceSocket(InprocAddress, null, ZSocketType.ROUTER);
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
            using (message)
            {
                id = message[0].ToString();
                var station = message[1].ToString();
                if (!StationProxy.TryGetValue(station, out item))
                {
                    logger.Error("({0})站点[{1}]不存在.", id, station);
                    if (ZeroRPCTaskInfo.Tasks.TryGetValue(id, out var task))
                    {
                        task.TaskSource.SetResult(new MessageResult
                        {
                            ID = id,
                            State = MessageState.FormalError,
                            RuntimeStatus = ApiResultHelper.Helper.ArgumentError
                        });
                    }
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
                Interlocked.Increment(ref WaitCount);
            }
            else
            {
                logger.Trace("({0})发送到远程服务失败.{1}", id, error.Text);
                if (ZeroRPCTaskInfo.Tasks.TryGetValue(id, out var task))
                {
                    task.TaskSource.SetResult(new MessageResult
                    {
                        ID = id,
                        State = MessageState.NetworkError,
                        RuntimeStatus = ApiResultHelper.Helper.NetworkError
                    });
                }
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

            if (!long.TryParse(zeroResult.Requester, out long id))
            {
                return;
            }

            if (zeroResult.State == ZeroOperatorStateType.Runing)
            {
                logger.Trace("({0}).远程通知正在处理中");
                return;
            }

            Interlocked.Decrement(ref WaitCount);
            if (!ZeroRPCTaskInfo.Tasks.TryRemove(zeroResult.Requester, out var src))
            {
                logger.Trace("({0})接收到远程返回,原始请求者非支.", zeroResult.Requester);
                return;
            }
            if (JsonHelper.TryDeserializeObject<MessageResult>(json, out var result))
            {
                result.DataState = MessageDataState.ResultOffline;
                result.Result = src.Caller.Message.Result;
            }
            else
            {
                src.Caller.CheckState(zeroResult.State);
                result = new MessageResult
                {
                    ID = src.Caller.Message.ID,
                    State = src.Caller.Message.State,
                    Trace = src.Caller.Message.Trace,
                    Result = src.Caller.Message.Result,
                    RuntimeStatus = src.Caller.GetOperatorStatus(zeroResult.State)
                };
            }
            logger.Trace("({0})接收到远程返回.{1}", result.Result);
            try
            {
                if (!src.TaskSource.TrySetResult(result))
                {
                    logger.Trace("({0})接收到远程返回.但等待队列已销毁:", zeroResult.Requester);
                }
            }
            catch
            {
                logger.Trace("({0})接收到远程返回.但等待队列已销毁:", zeroResult.Requester);
            }
        }

        #endregion
    }
}

