using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
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
                if (!mg.TryInstall(Logger, name, "Api"))
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
                    Service.Open();
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
        /// 代理地址
        /// </summary>
        private string InprocAddress;


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
                        OnResult(pool.Sockets[0], new ZMessage
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

            IInlineMessage messageItem = new InlineMessage
            {
                ID = callItem.RequestId,
                ServiceName = Config.StationName,
                ApiName = callItem.ApiName,
                Argument = callItem.Argument
            };
            if (SmartSerializer.TryToMessage(callItem.Extend, out var item))
            {
                messageItem.Trace = item.Trace;
                messageItem.State = item.State;
            }
            //if (messageItem.Trace != null)
            //{
            //    messageItem.Trace.TraceId = callItem.RequestId;
            //    messageItem.Trace.CallId = callItem.GlobalId;
            //    messageItem.Trace.CallMachine = callItem.Requester;
            //    messageItem.Trace.Context = SmartSerializer.ToObject<StaticContext>(callItem.Context);
            //}

            try
            {
                _ = MessageProcessor.OnMessagePush(Service, messageItem, true, callItem);
            }
            catch (Exception e)
            {
                Logger.Exception(e);
                messageItem.RealState = MessageState.BusinessError;
                OnExecuestEnd(messageItem.ToStateResult(), callItem, ZeroOperatorStateType.LocalException);
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
            if (!(tag is ApiCallItem item))
            {
                return Task.FromResult(true);
            }
            var state = message.State switch
            {
                MessageState.Cancel => ZeroOperatorStateType.Pause,
                MessageState.Success => ZeroOperatorStateType.Ok,
                MessageState.NetworkError => ZeroOperatorStateType.NetworkError,
                MessageState.Failed => ZeroOperatorStateType.Failed,
                MessageState.BusinessError => ZeroOperatorStateType.LocalException,
                //case MessageState.None:
                //case MessageState.Accept:
                //case MessageState.NonSupport:
                _ => ZeroOperatorStateType.NonSupport,
            };
            OnExecuestEnd(message.ToMessageResult(true), item, state);
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
            string address;
            using (message)
            {
                message2 = message.Duplicate(1);
                address = message[0].ToString();
            }
            bool success;
            ZError error;
            using (message2)
            {
                success = socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
            }
            if (!success)
            {
                Logger.Error($"发送回复出错,远程地址{address} => {error.Name} : {error.Text}");
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="operatorState"></param>
        /// <returns></returns>
        private Task<bool> OnExecuestEnd(IMessageResult result, ApiCallItem item, ZeroOperatorStateType operatorState)
        {
            var frames = CreateFrames(result, item, operatorState);
            if (!SendToZeroCenter(new ZMessage(frames), out var state))
            {
                Interlocked.Increment(ref SendError);
                Logger.Error(state);
                LogRecorder.MonitorInfomation(state);
                return Task.FromResult(false);
            }
            else
            {
                Interlocked.Increment(ref SendCount);
                LogRecorder.MonitorDetails(state);
                return Task.FromResult(true);
            }
        }

        private List<byte[]> CreateFrames(IMessageResult result, ApiCallItem item, ZeroOperatorStateType operatorState)
        {
            var json = result.Result;
            result.Result = null;

            int i = 0;
            var des = new byte[10 + item.Originals.Count];
            des[i++] = (byte)(item.Originals.Count + (item.EndTag == ZeroFrameType.ResultFileEnd ? 7 : 6));
            des[i++] = (byte)operatorState;
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
                json.ToBytes(),
                SmartSerializer.SerializeResult(result).ToBytes()
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
            return msg;
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
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool SendToZeroCenter(ZMessage message, out string msg)
        {
            if (!CanLoop)
            {
                msg = "系统已关闭,发送失败";
                return false;
            }
            using (message)
            {
                try
                {
                    var socket = ZSocketEx.CreateOnceSocket(InprocAddress, null, null, ZSocketType.PAIR);
                    if (socket == null)
                    {
                        msg = "无法构造Socket";
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
                        msg = "发送成功";
                        return true;
                    }
                    msg = $"发送错误 {error.Text} : {error.Name}";
                }
                catch (Exception e)
                {
                    msg = $"发送异常 {e.Message}";
                }
            }
            return false;
        }

        #endregion

    }
}

