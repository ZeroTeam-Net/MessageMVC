using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///  ZeroMQ实现的RPC
    /// </summary>
    public sealed class ZeroEventReceiver : MessageReceiverBase, INetEvent
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ZeroEventReceiver() : base(nameof(ZeroEventReceiver))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessageReceiver.PosterName => nameof(ZeroRPCPoster);

        #region IMessageReceiver

        /// <summary>
        /// 初始化
        /// </summary>
        bool IMessageReceiver.Prepare()
        {
            ZeroRpcFlow.ZeroNetEvents.Add(OnZeroNetEvent);
            return true;
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            RunTaskCancel = token;
            while (!token.IsCancellationRequested)
            {
                Listen(pool);
            }
            return Task.FromResult(true);
        }

        #endregion

        #region 状态
        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError;

        /// <summary>
        ///     运行状态
        /// </summary>
        private int _realState;

        /// <summary>
        ///     运行状态
        /// </summary>
        public int RealState
        {
            get => _realState;
            set
            {
                if (_realState == value)
                {
                    return;
                }
                Interlocked.Exchange(ref _realState, value);
                Logger.Information(() => $"RealState : {StationRealState.Text(_realState)}");
            }
        }


        /// <summary>
        /// 取消标记
        /// </summary>
        private CancellationToken RunTaskCancel { get; set; }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        private bool CanLoop => ZeroRpcFlow.CanDo &&
            RunTaskCancel != null && !RunTaskCancel.IsCancellationRequested &&
            (RealState == StationRealState.BeginRun || RealState == StationRealState.Run);

        /// <summary>
        /// 心跳器
        /// </summary>
        private HeartManager Hearter => ZeroCenterProxy.Master;

        #endregion

        #region 配置

        /// <summary>
        /// 实例名称
        /// </summary>
        public string RealName { get; private set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        public byte[] Identity { get; private set; }


        /// <summary>
        /// 站点配置
        /// </summary>
        public StationConfig Config { get; set; }


        /// <summary>
        /// 站点选项
        /// </summary>
        private ZeroStationOption _option;

        /// <summary>
        /// 配置检查
        /// </summary>
        /// <returns></returns>
        private bool CheckConfig()
        {
            var name = Service.ServiceName;
            //取配置
            Config = ZeroRpcFlow.Config[name];

            if (Config == null)
            {
                var mg = new StationConfigManager(ZeroRpcFlow.Config.Master);
                if (!mg.TryInstall(Logger, name, "Notify"))
                {
                    return false;
                }
                Task.Delay(1000).Wait();
                Config = mg.LoadConfig(name);
                if (Config == null)
                {
                    Logger.Error("Station no find");
                    RealState = StationRealState.ConfigError;
                    return false;
                }
            }
            //Config.OnStateChanged = OnConfigStateChanged;
            switch (Config.State)
            {
                case ZeroCenterState.None:
                case ZeroCenterState.Stop:
                    Logger.Error("Station is stop");
                    RealState = StationRealState.Stop;
                    return false;
                //case ZeroCenterState.Failed:
                //    Logger.Error( "Station is failed");
                //    RealState = StationState.ConfigError;
                //    ConfigState = ZeroCenterState.Stop;
                //    return false;
                case ZeroCenterState.Remove:
                    Logger.Error("Station is remove");
                    RealState = StationRealState.Remove;
                    return false;
            }
            _option = ZeroRpcFlow.GetApiOption(name);
            if (_option.SpeedLimitModel == SpeedLimitType.None)
            {
                _option.SpeedLimitModel = SpeedLimitType.ThreadCount;
            }

            //timeout = MicroZeroApplication.Config.ApiTimeout * 1000;
            //switch (_option.SpeedLimitModel)
            //{
            //    default:
            //        ZeroTrace.SystemLog(name, "WaitCount", MicroZeroApplication.Config.MaxWait);
            //        checkWait = true;
            //        break;
            //    case SpeedLimitType.Single:
            //        ZeroTrace.SystemLog(name, "Single");
            //        checkWait = false;
            //        break;
            //}
            return true;
        }

        private Task OnZeroNetEvent(ZeroRpcOption config, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.CenterSystemStop:
                    Service.Close();
                    return Task.CompletedTask;
                case ZeroNetEventType.CenterSystemStart:
                    Service.Open();
                    return Task.CompletedTask;
                case ZeroNetEventType.CenterWorkerSoundOff:
                case ZeroNetEventType.CenterStationTrends:
                case ZeroNetEventType.CenterClientJoin:
                case ZeroNetEventType.CenterClientLeft:
                case ZeroNetEventType.CenterStationDocument:
                    return Task.CompletedTask;
            }
            if (!string.Equals(Service.ServiceName, e.EventConfig?.StationName, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            switch (e.Event)
            {
                case ZeroNetEventType.ConfigUpdate:
                case ZeroNetEventType.CenterStationUpdate:
                    Config = e.EventConfig;

                    break;
                case ZeroNetEventType.CenterStationPause:
                    Service.ConfigState = StationStateType.Failed;
                    Service.Close();
                    break;
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                case ZeroNetEventType.CenterStationStop:
                    Service.ConfigState = StationStateType.Stop;
                    Service.Close();
                    break;
                case ZeroNetEventType.CenterStationInstall:
                    if (Service.ConfigState >= StationStateType.Stop)
                    {
                        Service.ConfigState = StationStateType.Initialized;
                        Service.ResetStateMachine();
                    }
                    break;
                case ZeroNetEventType.CenterStationJoin:
                    Service.ConfigState = StationStateType.Initialized;
                    Service.ResetStateMachine();
                    Service.Open();
                    break;
                case ZeroNetEventType.CenterStationResume:
                    Service.Open();
                    break;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region 主循环

        /// <summary>
        /// POOL
        /// </summary>
        private IZmqPool pool;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            try
            {
                if (!ZeroRpcFlow.ZerCenterIsRun)
                    return Task.FromResult(false);
                if (!CheckConfig())
                    return Task.FromResult(false);
                Logger.Information("LoopBegin");

                var pSocket = ZSocketEx.CreateSubSocket(Config.WorkerCallAddress, Config.ServiceKey, Identity, "");
                if (pSocket == null)
                {
                    RealState = StationRealState.Failed;
                    return Task.FromResult(false);
                }
                pool = ZmqPool.CreateZmqPool();
                pool.Prepare(ZPollEvent.In, pSocket);

                if (!Hearter.HeartReady(Service.Name, RealName))
                {
                    RealState = StationRealState.Failed;
                    return Task.FromResult(false);
                }
                RealState = StationRealState.Run;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                RealState = StationRealState.Failed;
                return Task.FromResult(false);
            }
        }

        private bool Listen(IZmqPool pool)
        {
            try
            {
                if (!pool.Poll())
                {
                    return false;
                }
                if (pool.CheckIn(0, out var message))
                {
                    Interlocked.Increment(ref RecvCount);
                    Interlocked.Increment(ref CallCount);
                    if (!ApiCallItem.Unpack(message, out var item) || string.IsNullOrWhiteSpace(item.ApiName))
                    {
                        return false;
                    }
                    OnCall(item);
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Exception(e);
                return false;
            }

        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task IMessageReceiver.LoopComplete()
        {
            pool.Sockets[0].Disconnect(Config.WorkerCallAddress, out _);
            Hearter.HeartLeft(Service.Name, RealName);
            try
            {
                int num = 0;
                while (Listen(pool))
                {
                    Logger.Trace("处理堆积任务{0}", ++num);
                }
            }
            catch (Exception e)
            {
                Logger.Exception(e, "处理堆积任务出错{0}", Service.Name);
            }
            pool.Dispose();
            Logger.Information("LoopComplete");
            return Task.CompletedTask;
        }


        private void OnCall(ApiCallItem callItem)
        {
            //var messageItem = MessageHelper.Restore(callItem.ApiName, callItem.Station, callItem.Argument, callItem.RequestId, callItem.Context);

            IInlineMessage messageItem = new InlineMessage
            {
                ID = callItem.RequestId,
                ServiceName = Config.StationName,
                ApiName = callItem.ApiName,
                Argument = callItem.Argument,
                Context = SmartSerializer.ToObject<System.Collections.Generic.Dictionary<string, string>>(callItem.Context)
            };
            if (SmartSerializer.TryToMessage(callItem.Extend, out var item))
            {
                messageItem.Trace = item.Trace;
                messageItem.State = item.State;
            }
            if (messageItem.Trace != null)
            {
                messageItem.Trace.TraceId = callItem.RequestId;
                messageItem.Trace.CallId = callItem.GlobalId;
                messageItem.Trace.CallMachine = callItem.Requester;
            }

            try
            {
                _ = MessageProcessor.OnMessagePush(Service, messageItem, true, callItem);
            }
            catch (Exception e)
            {
                Logger.Exception(e);
                messageItem.RealState = MessageState.BusinessError;
            }
        }

        #endregion

    }
}

