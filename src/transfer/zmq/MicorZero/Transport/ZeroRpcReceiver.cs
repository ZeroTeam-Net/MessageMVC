using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///  ZeroMQ实现的RPC
    /// </summary>
    public sealed class ZeroRpcReceiver : MessageReceiverBase, IServiceReceiver
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ZeroRpcReceiver() : base(nameof(ZeroRpcReceiver))
        {
        }
        #region 控制反转

        /// <summary>
        /// 初始化
        /// </summary>
        bool IMessageReceiver.Prepare()
        {
            ZeroRpcFlow.ZeroNetEvents.Add(OnZeroNetEvent);
            return CheckConfig();
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            RunTaskCancel = token;
            while (CanLoop)
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
                Logger.Information(() => $"{nameof(RealState)} : {StationRealState.Text(_realState)}");
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

        /*// <summary>
        /// 超时时长
        /// </summary>
        private int timeout;
        /// <summary>
        /// 是否检查Task超时
        /// </summary>
        bool checkWait;*/

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
                if (mg.TryInstall(Logger, name, "Api"))
                {
                    Config = mg.LoadConfig(name);
                    if (Config == null)
                    {
                        Logger.Error("Station no find");
                        RealState = StationRealState.ConfigError;
                        return false;
                    }
                }
                Config.State = ZeroCenterState.Run;
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
            InprocAddress = $"inproc://{name}_{RandomCode.Generate(4)}.req";


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
                    Service.Start();
                    return Task.CompletedTask;
                case ZeroNetEventType.CenterWorkerSoundOff:
                case ZeroNetEventType.CenterStationTrends:
                case ZeroNetEventType.CenterClientJoin:
                case ZeroNetEventType.CenterClientLeft:
                case ZeroNetEventType.CenterStationDocument:
                    return Task.CompletedTask;

            }
            if (e.EventConfig?.StationName != Service.ServiceName)
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
                    Service.Start();
                    break;
                case ZeroNetEventType.CenterStationResume:
                    Service.Start();
                    break;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 代理地址
        /// </summary>
        private string InprocAddress = "inproc://ApiProxy.req";
        private IZmqPool pool;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.LoopBegin()
        {
            try
            {
                Logger.Information("LoopBegin");

                var pSocket = ZSocketEx.CreatePoolSocket(Config.WorkerCallAddress, Config.ServiceKey, ZSocketType.PULL, Identity);
                if (pSocket == null)
                {
                    RealState = StationRealState.Failed;
                    return Task.FromResult(false);
                }
                pool = ZmqPool.CreateZmqPool();
                pool.Prepare(ZPollEvent.In,
                    ZSocketEx.CreatePoolSocket(Config.WorkerResultAddress, Config.ServiceKey, ZSocketType.DEALER, Identity),
                    pSocket,
                    ZSocketEx.CreateServiceSocket(InprocAddress, null, ZSocketType.ROUTER));

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
                    //Service.OnLoopIdle();
                    return false;
                }
                //对Result端口的返回的丢弃处理
                if (pool.CheckIn(0, out var message))
                {
                    message?.Dispose();
                }

                if (pool.CheckIn(1, out message))
                {
                    Interlocked.Increment(ref RecvCount);
                    Interlocked.Increment(ref CallCount);
                    if (!ApiCallItem.Unpack(message, out var item) || string.IsNullOrWhiteSpace(item.ApiName))
                    {
                        SendToZeroCenter(new ZMessage
                        {
                            new ZFrame(item.Caller),
                            new ZFrame(LayoutErrorFrame),
                            new ZFrame(item.Requester),
                            new ZFrame(Config.ServiceKey)
                        });
                        return false;
                    }
                    OnCall(item);
                }
                if (pool.CheckIn(2, out message))
                {
                    OnResult(pool.Sockets[0], message);
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

            var item = JsonHelper.DeserializeObject<InlineMessage>(callItem.Extend);
            var messageItem = new InlineMessage
            {
                ID = callItem.RequestId,
                ServiceName = callItem.Station,
                ApiName = callItem.ApiName,
                Argument = callItem.Argument,
                Trace = item.Trace,
                State = item.State
            };
            if (messageItem.Trace != null)
            {
                messageItem.Trace.TraceId = callItem.RequestId;
                messageItem.Trace.CallId = callItem.GlobalId;
                messageItem.Trace.CallMachine = callItem.Requester;
                messageItem.Trace.Context = JsonHelper.DeserializeObject<ZeroContext>(callItem.Context);
            }

            switch (callItem.ApiName[0])
            {
                case '$':
                    messageItem.RuntimeStatus = ApiResultHelper.Helper.Ok;
                    OnExecuestEnd(messageItem, callItem, ZeroOperatorStateType.Ok);
                    return;
                    //case '*':
                    //    item.Result = MicroZeroApplication.TestFunc();
                    //    OnExecuestEnd(item, ZeroOperatorStateType.Ok);
                    //    return;
            }

            try
            {
                _ = MessageProcessor.OnMessagePush(Service, messageItem, true, callItem);
            }
            catch (Exception e)
            {
                Logger.Exception(e);
                messageItem.RuntimeStatus = ApiResultHelper.Helper.BusinessException;
                OnExecuestEnd(messageItem, callItem, ZeroOperatorStateType.LocalException);
            }
        }

        #endregion

        #region 返回

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        Task<bool> IMessageReceiver.OnResult(IInlineMessage message, object tag)
        {
            if (tag is ApiCallItem item)
            {
                ZeroOperatorStateType state;
                switch (message.State)
                {
                    case MessageState.Cancel:
                        state = ZeroOperatorStateType.Pause;
                        break;
                    case MessageState.Success:
                        state = ZeroOperatorStateType.Ok;
                        break;
                    case MessageState.NetworkError:
                        state = ZeroOperatorStateType.NetworkError;
                        break;
                    case MessageState.Failed:
                        state = ZeroOperatorStateType.Failed;
                        break;
                    case MessageState.Error:
                        state = ZeroOperatorStateType.LocalException;
                        break;
                    //case MessageState.None:
                    //case MessageState.Accept:
                    //case MessageState.NonSupport:
                    default:
                        state = ZeroOperatorStateType.NonSupport;
                        break;
                }
                OnExecuestEnd(message, item, state);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        private void OnResult(ZSocket socket, ZMessage message)
        {
            ZMessage message2;
            using (message)
            {
                message2 = message.Duplicate(1);
            }

            bool success;
            ZError error;
            using (message2)
            {
                success = socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
            }
            if (!success)
            {
                Logger.Error($"Send message err.{error.Name} : {error.Text}");
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private Task<bool> OnExecuestEnd(IInlineMessage message, ApiCallItem item, ZeroOperatorStateType state)
        {
            var result = message.ToMessageResult();
            result.Result = null;

            int i = 0;
            var des = new byte[10 + item.Originals.Count];
            des[i++] = (byte)(item.Originals.Count + (item.EndTag == ZeroFrameType.ResultFileEnd ? 7 : 6));
            des[i++] = (byte)state;
            des[i++] = ZeroFrameType.Requester;
            des[i++] = ZeroFrameType.RequestId;
            des[i++] = ZeroFrameType.CallId;
            des[i++] = ZeroFrameType.GlobalId;
            des[i++] = ZeroFrameType.ResultText;
            des[i++] = ZeroFrameType.ExtendText;
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToBytes(),
                item.RequestId.ToBytes(),
                item.CallId.ToBytes(),
                item.GlobalId.ToBytes(),
                message.Result.ToBytes(),
                result.ToJson().ToBytes()
            };
            if (item.EndTag == ZeroFrameType.ResultFileEnd)
            {
                des[i++] = ZeroFrameType.BinaryContent;
                msg.Add(item.Binary);
            }
            foreach (var org in item.Originals)
            {
                des[i++] = org.Key;
                msg.Add(org.Value);
            }
            des[i++] = ZeroFrameType.SerivceKey;
            msg.Add(Config.ServiceKey);
            des[i] = item.EndTag > 0 ? item.EndTag : ZeroFrameType.ResultEnd;
            var res = SendToZeroCenter(new ZMessage(msg));
            return Task.FromResult(res);
        }

        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            2,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.Requester,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.ResultEnd
        };

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool SendToZeroCenter(ZMessage message)
        {
            string msg = null;
            using (message)
            {
                if (!CanLoop)
                {
                    Logger.Error("Can`t send result,station is closed");
                    return false;
                }
                try
                {
                    var socket = ZSocketEx.CreateOnceSocket(InprocAddress, null, null, ZSocketType.PAIR);
                    if (socket == null)
                    {
                        Interlocked.Increment(ref SendError);
                        return false;
                    }
                    ZError error;
                    bool success;
                    using (socket)
                    {
                        success = socket.Send(message, out error);
                    }

                    if (success)
                    {
                        Interlocked.Increment(ref SendCount);
                        return true;
                    }
                    msg = $"Send result err.{error.Text} : {error.Name}";
                }
                catch (Exception e)
                {
                    msg = $"Send result exception.{e.Message}";
                }
                Logger.Error(msg);
                LogRecorder.MonitorTrace(msg);
                Interlocked.Increment(ref SendError);
                return false;
            }
        }

        #endregion

    }
}

/*// <summary>
/// 站点状态变更时调用
/// </summary>
void OnStationStateChanged(StationConfig config)
{
    while (RealState == StationState.Start || RealState == StationState.BeginRun || RealState == StationState.Closing)
        Thread.Sleep(10);
    //#if UseStateMachine
    switch (config.State)
    {
        case ZeroCenterState.Initialize:
        case ZeroCenterState.Start:
        case ZeroCenterState.Run:
        case ZeroCenterState.Pause:
            break;
        case ZeroCenterState.Failed:
            if (!CanLoop)
                break;
            //运行状态中，与ZeroCenter一起重启
            if (RealState == StationState.Run)
                RealState = StationState.Closing;
            ConfigState = StationStateType.Failed;
            return;
        //以下状态如在运行，均可自动关闭
        case ZeroCenterState.Closing:
        case ZeroCenterState.Closed:
        case ZeroCenterState.Destroy:
            if (RealState == StationState.Run)
                RealState = StationState.Closing;
            ConfigState = StationStateType.Closed;
            return;
        case ZeroCenterState.NoFind:
        case ZeroCenterState.Remove:
            if (RealState == StationState.Run)
                RealState = StationState.Closing;
            ConfigState = StationStateType.Remove;
            return;
        default:
            //case ZeroCenterState.None:
            //case ZeroCenterState.Stop:
            if (RealState == StationState.Run)
                RealState = StationState.Closing;
            ConfigState = StationStateType.Stop;
            return;
    }
    if (!ZeroApplication.CanDo)
    {
        return;
    }
    //可启动状态检查是否未运行
    if (CanLoop)//这是一个可以正常运行的状态，无需要改变
        return;
    //是否未进行初始化
    if (ConfigState == StationStateType.None)
        Initialize();
    else
        ConfigState = StationStateType.Initialized;

    Task.Run(Start);
    //#else
    //            ConfigState = config.State;
    //            if (RealState < StationState.Start || RealState > StationState.Closing)
    //            {
    //                if (config.State <= ZeroCenterState.Pause && ZeroApplication.CanDo)
    //                {
    //                    ZeroTrace.SystemLog(StationName, $"Start by config state({config.State}) changed");
    //                    Start();
    //                }
    //            }
    //            else
    //            {
    //                if (config.State >= ZeroCenterState.Failed || !ZeroApplication.CanDo)
    //                {
    //                    ZeroTrace.SystemLog(StationName, $"Close by config state({config.State}) changed");
    //                    Close();
    //                }
    //            }
    //#endif
}*/
