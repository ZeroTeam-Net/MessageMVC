using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant;
using Agebull.EntityModel.Common;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///  ZeroMQ实现的RPC
    /// </summary>
    public sealed class ZmqRpcTransport : IRpcTransfer
    {

        #region 控制反转

        /// <summary>
        /// 初始化
        /// </summary>
        void INetTransfer.Initialize()
        {
            MicroZeroApplication.ZeroNetEvents.Add(OnZeroNetEvent);
            MicroZeroApplication.Transports.Add(this);
        }

        /// <summary>
        /// 将要开始
        /// </summary>
        bool INetTransfer.Prepare()
        {
            if (!MicroZeroApplication.ZerCenterIsRun)
                return false;
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
        /// <summary>
        /// 析构
        /// </summary>
        void IDisposable.Dispose()
        {
            MicroZeroApplication.Transports.Remove(this);
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
                    return;
                Interlocked.Exchange(ref _realState, value);
                ZeroTrace.SystemLog(Name, nameof(RealState), StationState.Text(_realState));
            }
        }


        /// <summary>
        /// 取消标记
        /// </summary>
        private CancellationToken RunTaskCancel { get; set; }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        private bool CanLoop => MicroZeroApplication.CanDo && 
            RunTaskCancel != null && !RunTaskCancel.IsCancellationRequested &&
            (RealState == StationState.BeginRun || RealState == StationState.Run);

        /// <summary>
        /// 心跳器
        /// </summary>
        private HeartManager Hearter => ZeroCenterProxy.Master;

        #endregion
        #region 配置

        /// <summary>
        /// 超时时长
        /// </summary>
        private int timeout;
        /// <summary>
        /// 是否检查Task超时
        /// </summary>
        bool checkWait;

        ///<inheritdoc/>
        public IService Service { get; set; }


        ///<inheritdoc/>
        public string Name { get; set; }


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
        ZeroStationOption _option;

        /// <summary>
        /// 配置检查
        /// </summary>
        /// <returns></returns>
        bool CheckConfig()
        {
            //取配置
            Config = MicroZeroApplication.Config[Name];

            if (Config == null)
            {
                var mg = new ConfigManager(MicroZeroApplication.Config.Master);
                if (mg.TryInstall(Name, "Api"))
                {
                    Config = mg.LoadConfig(Name);
                    if (Config == null)
                    {
                        ZeroTrace.WriteError(Name, "Station no find");
                        RealState = StationState.ConfigError;
                        return false;
                    }
                    ZeroTrace.SystemLog(Name, "successfully");
                }
                Config.State = ZeroCenterState.Run;
            }
            //Config.OnStateChanged = OnConfigStateChanged;
            switch (Config.State)
            {
                case ZeroCenterState.None:
                case ZeroCenterState.Stop:
                    ZeroTrace.WriteError(Name, "Station is stop");
                    RealState = StationState.Stop;
                    return false;
                //case ZeroCenterState.Failed:
                //    ZeroTrace.WriteError(Name, "Station is failed");
                //    RealState = StationState.ConfigError;
                //    ConfigState = ZeroCenterState.Stop;
                //    return false;
                case ZeroCenterState.Remove:
                    ZeroTrace.WriteError(Name, "Station is remove");
                    RealState = StationState.Remove;
                    return false;
            }
            InprocAddress = $"inproc://{Name}_{RandomOperate.Generate(4)}.req";

            timeout = MicroZeroApplication.Config.ApiTimeout * 1000;

            _option = MicroZeroApplication.GetApiOption(Name);
            if (_option.SpeedLimitModel == SpeedLimitType.None)
                _option.SpeedLimitModel = SpeedLimitType.ThreadCount;

            switch (_option.SpeedLimitModel)
            {
                default:
                    ZeroTrace.SystemLog(Name, "WaitCount", MicroZeroApplication.Config.MaxWait);
                    checkWait = true;
                    break;
                case SpeedLimitType.Single:
                    ZeroTrace.SystemLog(Name, "Single");
                    checkWait = false;
                    break;
            }
            return true;
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

        private Task OnZeroNetEvent(MicroZeroRuntimeConfig config, ZeroNetEventArgument e)
        {
            if (e.EventConfig?.StationName != Service.ServiceName)
                return Task.CompletedTask;
            switch (e.Event)
            {
                case ZeroNetEventType.ConfigUpdate:
                case ZeroNetEventType.CenterStationUpdate:
                    if (e.EventConfig?.StationName == Service.ServiceName)
                        Config = e.EventConfig;
                    break;
                //case ZeroNetEventType.CenterStationLeft:
                //case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                case ZeroNetEventType.CenterStationStop:
                    Service.ConfigState = ZeroTeam.MessageMVC.StationStateType.Failed;
                    break;
                case ZeroNetEventType.CenterSystemStart:
                case ZeroNetEventType.CenterStationInstall:
                case ZeroNetEventType.CenterStationResume:
                    Service.ConfigState = ZeroTeam.MessageMVC.StationStateType.Run;
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

        /// <summary>
        /// 本地代理
        /// </summary>
        private ZSocket _proxyServiceSocket;

        IZmqPool pool;

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        bool INetTransfer.LoopBegin()
        {
            try
            {
                ZeroTrace.SystemLog(Name, "Task", "start", RealName);

                var pSocket = ZSocketEx.CreatePoolSocket(Config.WorkerCallAddress, Config.ServiceKey, ZSocketType.PULL, Identity);
                if (pSocket == null)
                {
                    RealState = StationState.Failed;
                    return false;
                }
                pool = ZmqPool.CreateZmqPool();
                pool.Prepare(ZPollEvent.In,
                    ZSocketEx.CreatePoolSocket(Config.WorkerResultAddress, Config.ServiceKey, ZSocketType.DEALER, Identity),
                    pSocket,
                    ZSocketEx.CreateServiceSocket(InprocAddress, null, ZSocketType.ROUTER));

                if (!Hearter.HeartReady(Name, RealName))
                {
                    RealState = StationState.Failed;
                    return false;
                }
                RealState = StationState.Run;
                return true;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                RealState = StationState.Failed;
                return false;
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
                    message?.Dispose();

                if (pool.CheckIn(1, out message))
                {
                    Interlocked.Increment(ref RecvCount);
                    Interlocked.Increment(ref CallCount);
                    if (!ApiCallItem.Unpack(message, out var item) || string.IsNullOrWhiteSpace(item.ApiName))
                    {
                        SendLayoutErrorResult(item);
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
        void INetTransfer.LoopComplete()
        {
            pool.Sockets[0].Disconnect(Config.WorkerCallAddress, out _);
            Hearter.HeartLeft(Name, RealName);
            ZeroTrace.SystemLog(Name, "closing");
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
                LogRecorder.Exception(e, "处理堆积任务出错{0}", Name);
            }
            pool.Dispose();
            ZeroTrace.SystemLog(Name, "Task", "end", RealName);
        }


        private void OnCall(ApiCallItem item)
        {
            switch (item.ApiName[0])
            {
                case '$':
                    OnExecuestEnd(ApiResult.SucceesJson, item, ZeroOperatorStateType.Ok);
                    return;
                    //case '*':
                    //    item.Result = MicroZeroApplication.TestFunc();
                    //    OnExecuestEnd(item, ZeroOperatorStateType.Ok);
                    //    return;
            }

            var arg = new MessageItem
            {
                Title = item.ApiName,
                Topic = item.Station,
                Content = item.Argument,
                Context = item.Context
            };
            try
            {
                _ = MessageProcess.OnMessagePush(Service, arg, item);
            }
            catch (Exception e)
            {
                OnError(e, arg, item);
            }
        }

        #endregion


        #region 返回

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        public void OnResult(IMessageItem message, object tag)
        {
            OnExecuestEnd(message.Result,
                (ApiCallItem)tag,
                message.State == MessageState.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed);
        }

        /// <summary>
        /// 错误 
        /// </summary>
        /// <returns></returns>
        public void OnError(Exception exception, IMessageItem message, object tag)
        {
            LogRecorder.Exception(exception);
            OnExecuestEnd(ApiResultIoc.LocalExceptionJson,
                (ApiCallItem)tag,
                ZeroOperatorStateType.LocalException);
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
                ZeroTrace.WriteError(Name, error.Text, error.Name);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal bool OnExecuestEnd(string result, ApiCallItem item, ZeroOperatorStateType state)
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
            return SendResult(new ZMessage(msg));
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
        /// <param name="item"></param>
        /// <returns></returns>
        internal void SendLayoutErrorResult(ApiCallItem item)
        {
            if (item == null)
            {
                Interlocked.Increment(ref SendError);
                return;
            }
            SendResult(new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(LayoutErrorFrame),
                new ZFrame(item.Requester),
                new ZFrame(Config.ServiceKey)
            });
        }


        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool SendResult(ZMessage message)
        {
            using (message)
            {
                if (!CanLoop)
                {
                    ZeroTrace.WriteError(Name, "Can`t send result,station is closed");
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

                    ZeroTrace.WriteError(Name, error.Text, error.Name);
                    LogRecorder.MonitorTrace(() => $"{Name}({socket.Endpoint}) : {error.Text}");
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "ApiStation.SendResult");
                    LogRecorder.MonitorTrace(() => $"Exception : {Name} : {e.Message}");
                }
            }
            Interlocked.Increment(ref SendError);
            return false;
        }

        #endregion

    }
}
