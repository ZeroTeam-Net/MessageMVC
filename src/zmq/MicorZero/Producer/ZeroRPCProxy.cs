using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Agebull.Common.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.EntityModel.Common;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     API客户端代理
    /// </summary>
    public class ZeroRPCProxy : IFlowMiddleware
    {
        #region IFlowMiddleware 

        /// <summary>
        /// 实例名称
        /// </summary>
        string IFlowMiddleware.RealName => "ZeroRPCProxy";

        /// <summary>
        /// 等级
        /// </summary>
        int IFlowMiddleware.Level => 0;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            ZeroRpcFlow.ZeroNetEvents.Add(OnZeroNetEvent);
        }

        /// <summary>
        /// 等待数量
        /// </summary>
        public int WaitCount;

        /// <summary>
        /// 实例
        /// </summary>
        public static ZeroRPCProxy Instance { get; private set; }


        #endregion
        #region Socket

        /// <summary>
        /// 代理地址
        /// </summary>
        private const string InprocAddress = "inproc://ZeroRPCProxy.req";

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket _proxyServiceSocket;


        /// <summary>
        /// 取得连接器
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public static ZSocketEx GetSocket(string station)
        {
            return GetSocket(station, RandomOperate.Generate(8));
        }

        /// <summary>
        /// 取得连接器
        /// </summary>
        /// <param name="station"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ZSocketEx GetSocket(string station, string name)
        {
            if (!StationProxy.TryGetValue(station, out var item))// || item.Config.State != ZeroCenterState.Run
                return null;
            return ZSocketEx.CreateOnceSocket(InprocAddress, item.Config.ServiceKey, name.ToZeroBytes(), ZSocketType.PAIR);
        }

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
                        _isChanged = true;
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

        /// <summary>
        /// Pool对象
        /// </summary>
        private IZmqPool _zmqPool;

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
                            ZSocketType.DEALER, ZSocketHelper.CreateIdentity(false, item.Config.Name));
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

        private ZSocketEx CreateProxySocket(ZeroRPCCaller caller)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(caller.Station, out caller.Config))
            {
                caller.Result = ApiResultIoc.NoFindJson;
                caller.State = ZeroOperatorStateType.NotFind;
                return null;
            }

            if (caller.Config.State != ZeroCenterState.None && caller.Config.State != ZeroCenterState.Run)
            {
                caller.Result = caller.Config.State == ZeroCenterState.Pause
                    ? ApiResultIoc.PauseJson
                    : ApiResultIoc.NotSupportJson;
                caller.State = ZeroOperatorStateType.Pause;
                return null;
            }

            caller.State = ZeroOperatorStateType.None;

            return ZSocketEx.CreateOnceSocket(InprocAddress, null, caller.Name.ToString().ToZeroBytes(), ZSocketType.PAIR);
        }

        #endregion

        #region RPC

        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            0,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.ResultEnd
        };

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private void SendLayoutErrorResult(byte[] caller)
        {
            SendResult(new ZMessage
            {
                new ZFrame(caller),
                new ZFrame(LayoutErrorFrame)
            });
            Interlocked.Decrement(ref WaitCount);
        }

        private static readonly byte[] NetErrorFrame = new byte[]
        {
            0,
            (byte) ZeroOperatorStateType.NetError,
            ZeroFrameType.ResultEnd
        };

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private void SendNetErrorResult(byte[] caller)
        {
            SendResult(new ZMessage
            {
                new ZFrame(caller),
                new ZFrame(NetErrorFrame)
            });
            Interlocked.Decrement(ref WaitCount);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private void SendResult(ZMessage message)
        {
            try
            {
                ZError error;
                lock (_proxyServiceSocket)
                {
                    using (message)
                    {
                        if (_proxyServiceSocket.Send(message, out error))
                            return;
                    }
                }

                LogRecorder.MonitorTrace(() => $"ApiProxy({error.Name}) : {error.Text}");
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "ApiStation.SendResult");
                LogRecorder.MonitorTrace(e.Message);
            }
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 构造
        /// </summary>
        public void Start()
        {
            Instance = this;
            Task.Run(Loop);
        }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        protected bool CanLoopEx => WaitCount > 0 || ZeroFlowControl.CanDo;
        void Prepare()
        {
            var identity = GlobalContext.ServiceRealName.ToZeroBytes();
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
            ZeroTrace.SystemLog("ApiProxy", "Run");
            _isChanged = true;
        }
        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public bool Loop()
        {
            Prepare();

            ZeroRPCProducer.Instance.State = StationStateType.Run;
            while (CanLoopEx)
            {
                try
                {
                    if (_isChanged)
                    {
                        _zmqPool = CreatePool();
                    }
                    if (!_zmqPool.Poll())
                    {
                        continue;
                    }

                    if (_zmqPool.CheckIn(0, out var message))
                    {
                        OnLocalCall(message);
                    }

                    for (int idx = 1; idx < _zmqPool.Size; idx++)
                    {
                        if (_zmqPool.CheckIn(idx, out message))
                            OnRemoteResult(message);
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                }
            }
            ZeroRPCProducer.Instance.State = StationStateType.Closed;
            return true;
        }

        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        public void End()
        {
            _proxyServiceSocket.Dispose();
            _zmqPool?.Dispose();

            foreach (var proxyItem in StationProxy.Values)
            {
                proxyItem.Socket?.Dispose();
            }
            StationProxy.Clear();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="message"></param>
        private void OnLocalCall(ZMessage message)
        {
            Interlocked.Increment(ref WaitCount);
            using (message)
            {
                var station = message[1].ToString();
                if (!StationProxy.TryGetValue(station, out var item))
                {
                    SendLayoutErrorResult(message[0].ReadAll());
                    return;
                }
                var message2 = message.Duplicate(2);
                bool success;
                ZError error;
                using (message2)
                {
                    success = item.Socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
                }

                if (success)
                    return;
                ZeroTrace.WriteError("ApiProxy . OnLocalCall", error.Text);
                var caller = message[0].ReadAll();
                SendNetErrorResult(caller);
            }

        }

        private void OnRemoteResult(ZMessage message)
        {
            long id;
            ZeroResult result;
            using (message)
            {
                result = ZeroResultData.Unpack<ZeroResult>(message, true, (res, type, bytes) =>
                {
                    switch (type)
                    {
                        case ZeroFrameType.ResultText:
                            res.Result = ZeroNetMessage.GetString(bytes);
                            return true;
                        case ZeroFrameType.BinaryContent:
                            res.Binary = bytes;
                            return true;
                    }
                    return false;
                });
                if (result.State != ZeroOperatorStateType.Runing)
                    Interlocked.Decrement(ref WaitCount);
                if (!long.TryParse(result.Requester, out id))
                {
                    using (var message2 = message.Duplicate())
                    {
                        message2.Prepend(new ZFrame(result.Requester ?? result.RequestId));
                        SendResult(message2);
                    }
                    return;
                }
            }

            if (!Tasks.TryGetValue(id, out var src))
                return;
            src.Caller.State = result.State;
            if (result.State == ZeroOperatorStateType.Runing)
            {
                LogRecorder.Trace($"task:({id})=>Runing");
                return;
            }
            Tasks.TryRemove(id, out _);
            LogRecorder.Trace("OnRemoteResult");
            if (!src.TaskSource.TrySetResult(result))
            {
                LogRecorder.Error($"task:({id})=>Failed result({JsonHelper.SerializeObject(result)})");
            }
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
        internal Task<ZeroResult> CallZero(ZeroRPCCaller caller, byte[] description, params byte[][] args)
        {
            return CallZero(caller, description, (IEnumerable<byte[]>)args);
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Task<ZeroResult> CallZero(ZeroRPCCaller caller, byte[] description, IEnumerable<byte[]> args)
        {
            var socket = CreateProxySocket(caller);
            if (socket == null)
            {
                return Task.FromResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.NoWorker,
                });
            }
            var info = new TaskInfo
            {
                Caller = caller,
                Start = DateTime.Now
            };
            if (!Tasks.TryAdd(caller.Name, info))
            {
                return Task.FromResult(new ZeroResult
                {
                    State = ZeroOperatorStateType.NoWorker,
                });
            }

            using (MonitorScope.CreateScope("SendToZero"))
            {
                using var message = new ZMessage
                {
                    new ZFrame(caller.Station),
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
                ZError error;
                using (socket)
                {
                    res = socket.Send(message, out error);
                }
                if (!res)
                {
                    return Task.FromResult(new ZeroResult
                    {
                        State = ZeroOperatorStateType.LocalSendError,
                        ZmqError = error
                    });
                }
            }

            info.TaskSource = new TaskCompletionSource<ZeroResult>();
            return info.TaskSource.Task;
        }

        internal readonly ConcurrentDictionary<long, TaskInfo> Tasks = new ConcurrentDictionary<long, TaskInfo>();


        internal class TaskInfo
        {
            /// <summary>
            /// TaskCompletionSource
            /// </summary>
            public TaskCompletionSource<ZeroResult> TaskSource;

            /// <summary>
            /// 任务开始时间
            /// </summary>
            public DateTime Start;

            /// <summary>
            /// 调用对象
            /// </summary>
            public ZeroRPCCaller Caller;
        }

        #endregion
    }
}

