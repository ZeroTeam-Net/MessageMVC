using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
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
        string IZeroMiddleware.Name => "ZeroRPCProxy";

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => 0;

        ILogger logger;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
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
        public void Start()
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
            ZeroRPCPoster.Instance.State = StationStateType.Run;
            while (CanLoopEx)
            {
                Listen();
            }
            logger.Information("Close");
            ZeroRPCPoster.Instance.State = StationStateType.Closed;
            return true;
        }

        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        public void End()
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
            ulong id;
            ZMessage message2;
            StationProxyItem item;
            using (message)
            {
                id = ulong.Parse(message[0].ToString());
                var station = message[1].ToString();
                if (!StationProxy.TryGetValue(station, out item))
                {
                    if (Tasks.TryGetValue(id, out var task))
                    {
                        task.Caller.Message.State = MessageState.FormalError;
                        task.Caller.Message.Result = ApiResultHelper.ArgumentErrorJson;
                        task.TaskSource.SetResult(ZeroOperatorStateType.ArgumentInvalid);
                    }
                    return;
                }
                message2 = message.Duplicate(2);
            }
            logger.Trace("OnLocalCall : {0}", id);
            bool success;
            ZError error;
            using (message2)
            {
                success = item.Socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
            }

            if (success)
            {
                Interlocked.Increment(ref WaitCount);
            }
            else
            {
                LogRecorder.Trace(error.Text);
                if (Tasks.TryGetValue(id, out var task))
                {
                    task.Caller.Message.State = MessageState.NetError;
                    task.Caller.Message.Result = ApiResultHelper.NetworkErrorJson;
                    task.TaskSource.SetResult(ZeroOperatorStateType.NetError);
                }
            }
        }



        private void OnRemoteResult(ZMessage message)
        {
            ZeroResult result = ZeroResultData.Unpack<ZeroResult>(message, true, (res, type, bytes) =>
            {
                switch (type)
                {
                    case ZeroFrameType.ResultText:
                        res.Result = ZeroNetMessage.GetString(bytes);
                        return true;
                        //case ZeroFrameType.BinaryContent:
                        //    res.Binary = bytes;
                        //    return true;
                }
                return false;
            });

            if (!ulong.TryParse(result.Requester, out ulong id))
            {
                LogRecorder.Trace(() => $"[OnRemoteResult]  Requester error:{result.Requester}");
                return;
            }

            if (result.State == ZeroOperatorStateType.Runing)
            {
                LogRecorder.Trace(() => $"[OnRemoteResult] task:({id})=>Runing");
                return;
            }

            Interlocked.Decrement(ref WaitCount);
            if (!Tasks.TryGetValue(id, out var src))
            {
                LogRecorder.Trace(() => $"[OnRemoteResult]  Requester error:{result.Requester}");
                return;
            }
            Tasks.TryRemove(id, out _);
            src.Caller.CheckState(result.State);
            src.Caller.CheckResult(result.Result, result.State);
            if (!src.TaskSource.TrySetResult(result.State))
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
        internal Task<ZeroOperatorStateType> CallZero(ZeroCaller caller, byte[] description, params byte[][] args)
        {
            var socket = CreateProxySocket(caller);
            if (socket == null)
            {
                return Task.FromResult(ZeroOperatorStateType.NoWorker);
            }
            var info = new TaskInfo
            {
                Caller = caller
            };
            if (!Tasks.TryAdd(caller.ID, info))
            {
                caller.Message.ResultData = ApiResultHelper.Helper.Unavailable;
                caller.Message.State = MessageState.Error;
                return Task.FromResult(ZeroOperatorStateType.Unavailable);
            }

            using (MonitorScope.CreateScope("SendToZero"))
            {
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
                ZError error;
                using (socket)
                {
                    res = socket.Send(message, out error);
                }
                if (!res)
                {
                    caller.Message.Result = ApiResultHelper.NetworkErrorJson;
                    caller.Message.State = MessageState.NetError;
                    return Task.FromResult(ZeroOperatorStateType.LocalSendError);
                }
            }

            info.TaskSource = new TaskCompletionSource<ZeroOperatorStateType>();
            return info.TaskSource.Task;
        }

        private ZSocketEx CreateProxySocket(ZeroCaller caller)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(caller.Message.ServiceName, out caller.Config))
            {
                caller.Message.Result = ApiResultHelper.NoFindJson;
                caller.Message.State = MessageState.NoSupper;
                return null;
            }

            if (caller.Config.State != ZeroCenterState.None && caller.Config.State != ZeroCenterState.Run)
            {
                caller.Message.Result = caller.Config.State == ZeroCenterState.Pause
                    ? ApiResultHelper.PauseJson
                    : ApiResultHelper.NotSupportJson;
                caller.Message.State = MessageState.NoSupper;
                return null;
            }

            caller.Message.State = MessageState.None;

            return ZSocketEx.CreateOnceSocket(InprocAddress, null, caller.ID.ToString().ToBytes(), ZSocketType.PAIR);
        }

        internal readonly ConcurrentDictionary<ulong, TaskInfo> Tasks = new ConcurrentDictionary<ulong, TaskInfo>();


        internal class TaskInfo
        {
            /// <summary>
            /// TaskCompletionSource
            /// </summary>
            public TaskCompletionSource<ZeroOperatorStateType> TaskSource;

            /// <summary>
            /// 调用对象
            /// </summary>
            public ZeroCaller Caller;
        }

        #endregion
    }
}

