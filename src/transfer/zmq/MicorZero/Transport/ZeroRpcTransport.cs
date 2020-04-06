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
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///  ZeroMQ实现的RPC
    /// </summary>
    public sealed class ZeroRpcTransport : NetTransferBase, IServiceTransfer
    {
        #region 控制反转

        /// <summary>
        /// 初始化
        /// </summary>
        bool INetTransfer.Prepare()
        {
            ZeroRpcFlow.ZeroNetEvents.Add(OnZeroNetEvent);
            return CheckConfig();
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public Task<bool> Loop(CancellationToken token)
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
                ZeroTrace.SystemLog(Service.Name, nameof(RealState), StationRealState.Text(_realState));
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
                if (mg.TryInstall(name, "Api"))
                {
                    Config = mg.LoadConfig(name);
                    if (Config == null)
                    {
                        ZeroTrace.WriteError(name, "Station no find");
                        RealState = StationRealState.ConfigError;
                        return false;
                    }
                    ZeroTrace.SystemLog(name, "successfully");
                }
                Config.State = ZeroCenterState.Run;
            }
            //Config.OnStateChanged = OnConfigStateChanged;
            switch (Config.State)
            {
                case ZeroCenterState.None:
                case ZeroCenterState.Stop:
                    ZeroTrace.WriteError(name, "Station is stop");
                    RealState = StationRealState.Stop;
                    return false;
                //case ZeroCenterState.Failed:
                //    ZeroTrace.WriteError(name, "Station is failed");
                //    RealState = StationState.ConfigError;
                //    ConfigState = ZeroCenterState.Stop;
                //    return false;
                case ZeroCenterState.Remove:
                    ZeroTrace.WriteError(name, "Station is remove");
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
                        ((ZeroService)Service).ResetStateMachine();
                    }
                    break;
                case ZeroNetEventType.CenterStationJoin:
                    Service.ConfigState = StationStateType.Initialized;
                    ((ZeroService)Service).ResetStateMachine();
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
        Task<bool> INetTransfer.LoopBegin()
        {
            try
            {
                ZeroTrace.SystemLog(Service.Name, "Task", "start", RealName);

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
                LogRecorder.Exception(ex);
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
                LogRecorder.Exception(e);
                return false;
            }

        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        Task INetTransfer.LoopComplete()
        {
            pool.Sockets[0].Disconnect(Config.WorkerCallAddress, out _);
            Hearter.HeartLeft(Service.Name, RealName);
            ZeroTrace.SystemLog(Service.Name, "closing");
            try
            {
                int num = 0;
                while (Listen(pool))
                {
                    LogRecorder.Trace("处理堆积任务{0}", ++num);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "处理堆积任务出错{0}", Service.Name);
            }
            pool.Dispose();
            ZeroTrace.SystemLog(Service.Name, "Task", "end", RealName);
            return Task.CompletedTask;
        }


        private void OnCall(ApiCallItem callItem)
        {
            switch (callItem.ApiName[0])
            {
                case '$':
                    OnExecuestEnd(ApiResultHelper.SucceesJson, callItem, ZeroOperatorStateType.Ok);
                    return;
                    //case '*':
                    //    item.Result = MicroZeroApplication.TestFunc();
                    //    OnExecuestEnd(item, ZeroOperatorStateType.Ok);
                    //    return;
            }

            var messageItem = MessageHelper.Restore(callItem.ApiName, callItem.Station, callItem.Argument, callItem.RequestId, callItem.Context);
            messageItem.Trace.CallId = callItem.GlobalId;
            messageItem.Trace.LocalId = callItem.LocalId;
            messageItem.Extend = callItem.Extend;
            messageItem.Binary = callItem.Binary;
            try
            {
                _ = MessageProcessor.OnMessagePush(Service, messageItem, callItem);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                OnExecuestEnd(ApiResultHelper.LocalExceptionJson, callItem, ZeroOperatorStateType.LocalException);
            }
        }

        #endregion

        #region 返回

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        Task<bool> INetTransfer.OnResult(IMessageItem message, object tag)
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
                    case MessageState.NetError:
                        state = ZeroOperatorStateType.NetError;
                        break;
                    case MessageState.Failed:
                        state = ZeroOperatorStateType.Failed;
                        break;
                    case MessageState.Exception:
                        state = ZeroOperatorStateType.LocalException;
                        break;
                    //case MessageState.None:
                    //case MessageState.Accept:
                    //case MessageState.NoSupper:
                    default:
                        state = ZeroOperatorStateType.NotSupport;
                        break;
                }
                OnExecuestEnd(message.Result, item, state);
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
                ZeroTrace.WriteError(Service.Name, error.Text, error.Name);
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal Task<bool> OnExecuestEnd(string result, ApiCallItem item, ZeroOperatorStateType state)
        {
            int i = 0;
            var des = new byte[10 + item.Originals.Count];
            des[i++] = (byte)(item.Originals.Count + (item.EndTag == ZeroFrameType.ResultFileEnd ? 7 : 6));
            des[i++] = (byte)state;
            des[i++] = ZeroFrameType.Requester;
            des[i++] = ZeroFrameType.RequestId;
            des[i++] = ZeroFrameType.CallId;
            des[i++] = ZeroFrameType.GlobalId;
            des[i++] = ZeroFrameType.ResultText;
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToZeroBytes(),
                item.RequestId.ToZeroBytes(),
                item.CallId.ToZeroBytes(),
                item.GlobalId.ToZeroBytes(),
                result.ToZeroBytes()
            };
            if (item.EndTag == ZeroFrameType.ResultFileEnd)
            {
                des[i++] = ZeroFrameType.BinaryContent;
                msg.Add(item.Binary);
            }
            foreach (var org in item.Originals)
            {
                des[i++] = org.Key;
                msg.Add((org.Value));
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
            using (message)
            {
                if (!CanLoop)
                {
                    ZeroTrace.WriteError(Service.Name, "Can`t send result,station is closed");
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

                    ZeroTrace.WriteError(Service.Name, error.Text, error.Name);
                    LogRecorder.MonitorTrace(() => $"{Service.Name}({socket.Endpoint}) : {error.Text}");
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "ApiStation.SendResult");
                    LogRecorder.MonitorTrace(() => $"Exception : {Service.Name} : {e.Message}");
                }
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
