using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant;
using ZeroTeam.MessageMVC;
using Agebull.Common.Configuration;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     MicroZero流程控制器
    /// </summary>
    public partial class MicroZeroApplication : IFlowMiddleware
    {
        #region 配置

        /// <summary>
        ///     当前应用名称
        /// </summary>
        public static string AppName => ZeroFlowControl.AppName;

        /// <summary>
        ///     站点配置
        /// </summary>
        public static MicroZeroRuntimeConfig Config { get; set; }

        /// <summary>
        /// 工作模式
        /// </summary>
        public static ZeroWorkModel WorkModel { get; set; }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public void CheckOption(ZeroAppConfigRuntime config)
        {
            ZContext.Initialize();
            #region 配置组合

            var sec = ConfigurationManager.Get("Zero");
            if (sec == null)
                throw new Exception("无法找到主配置节点,路径为Zero,在zero.json或appsettings.json中设置");

            var opt = sec.Child<SocketOption>("socketOption");
            if (opt != null)
                ZSocket.Option = opt;
            ZSocket.Option.CheckOption();
            Config = new MicroZeroRuntimeConfig();
            var glc = sec.Child<MicroZeroConfig>("Global");
            if (glc != null)
                Config.CopyByEmpty(glc);
            var cfg = sec.Child<MicroZeroConfig>(AppName);
            if (cfg != null)
                Config.CopyByEmpty(cfg);


            #endregion

            #region ServiceName

            if (string.IsNullOrWhiteSpace(Config.StationName))
                Config.StationName = AppName;

            Config.ShortName = string.IsNullOrWhiteSpace(Config.ShortName)
                ? Config.StationName
                : Config.ShortName.Trim();

            if (string.IsNullOrWhiteSpace(Config.ServiceName))
            {
                Config.ServiceName = Config.StationName;
            }

            #endregion

            #region ZeroCenter

            ZeroCommandExtend.AppNameBytes = AppName.ToZeroBytes();

            Config.Master = Config.ZeroGroup[0];
            if (Config.ApiTimeout <= 1)
                Config.ApiTimeout = 60;
            #endregion
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        public static ZeroStationOption GetApiOption(string station)
        {
            return GetStationOption(station);
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        public static ZeroStationOption GetClientOption(string station)
        {
            return GetStationOption(station);
        }

        /// <summary>
        ///     配置校验
        /// </summary>
        private static ZeroStationOption GetStationOption(string station)
        {
            var sec = ConfigurationManager.Get("Zero");
            var option = sec.Child<ZeroStationOption>(station) ?? new ZeroStationOption();
            option.CopyByEmpty(Config);
            if (option.ApiTimeout <= 1)
                option.ApiTimeout = 60;

            if (option.SpeedLimitModel != SpeedLimitType.WaitCount)
                option.SpeedLimitModel = SpeedLimitType.Single;

            //if (option.TaskCpuMultiple <= 0)
            //    option.TaskCpuMultiple = 1;
            //else if (option.TaskCpuMultiple > 128)
            //    option.TaskCpuMultiple = 128;

            if (option.MaxWait < 0xFF)
                option.MaxWait = 0xFF;
            else if (option.MaxWait > 0xFFFFF)
                option.MaxWait = 0xFFFFF;

            return option;
        }
        #endregion

        #region State

        /// <summary>
        /// 实例名称
        /// </summary>
        string IFlowMiddleware.RealName => "MicroZeroFlow";

        /// <summary>
        /// 等级
        /// </summary>
        int IFlowMiddleware.Level => short.MinValue;

        /// <summary>
        ///     运行状态
        /// </summary>
        private static ZeroCenterState _state;

        /// <summary>
        ///     应用中心状态
        /// </summary>
        public static ZeroCenterState ZeroCenterState
        {
            get => _state;
            internal set
            {
                _state = value;
                if (IsAlive)
                {
                    MonitorStateMachine.SyncAppState();
                    ZeroTrace.SystemLog("ZeroApplication",
                        _state.ToString(),
                        MonitorStateMachine.StateMachine.GetTypeName());
                }
            }
        }


        /// <summary>
        /// 设置ZeroCenter与Application状态都为Failed
        /// </summary>
        public static void SetFailed()
        {
            ZeroCenterState = ZeroCenterState.Failed;
        }

        /// <summary>
        ///     应用中心是否正在运行
        /// </summary>
        public static bool ZerCenterIsRun => ZeroCenterState == ZeroCenterState.Run;

        /// <summary>
        ///     运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo => ZeroFlowControl.ApplicationIsRun && ZerCenterIsRun;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ZeroFlowControl.ApplicationState < StationState.Destroy;

        /// <summary>
        /// 中心事件监控对象
        /// </summary>
        static readonly ZeroEventMonitor SystemMonitor = new ZeroEventMonitor();

        #endregion

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            ZeroCenterProxy.Master = new ZeroCenterProxy(Config.Master);
            JoinCenter();
        }


        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            if (ZeroCenterState == ZeroCenterState.Start || ZeroCenterState == ZeroCenterState.Run)
                return false;
            ZeroCenterState = ZeroCenterState.Start;
            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", $"try connect zero center ({Config.Master.ManageAddress})...");
            if (!ZeroCenterProxy.Master.PingCenter())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "zero center can`t connection.");
                return false;
            }
            ZeroCenterState = ZeroCenterState.Run;
            if (!ZeroCenterProxy.Master.HeartJoin())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "zero center can`t join.");
                return false;
            }

            Config.ClearConfig();
            if (!ConfigManager.LoadAllConfig())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "station configs can`t loaded.");
                return false;
            }
            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", "be connected successfully,start local stations.");

            if (ZeroFlowControl.ApplicationState == (int)StationState.Run)
            {
                ZeroFlowControl.StartFailed();
                if (WorkModel == ZeroWorkModel.Service)
                {
                    var m = new ConfigManager(Config.Master);
                    m.UploadDocument();
                }
            }
            return true;
        }

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        public void Start()
        {
            SystemMonitor.Start();
            if (ZeroFlowControl.ApplicationState == (int)StationState.Run && WorkModel == ZeroWorkModel.Service)
            {
                var m = new ConfigManager(Config.Master);
                m.UploadDocument();
            }
        }
        #endregion

        #region Destroy

        /// <summary>
        ///     关闭
        /// </summary>
        public void Dispose()
        {
            ZContext.Destroy();
        }

        #endregion

        #region Event

        /// <summary>
        /// 站点事件发生
        /// </summary>
        public static List<Func<MicroZeroRuntimeConfig, ZeroNetEventArgument, Task>> ZeroNetEvents = new List<Func<MicroZeroRuntimeConfig, ZeroNetEventArgument, Task>>();

        /// <summary>
        /// 发出事件
        /// </summary>
        public static void InvokeEvent(ZeroNetEventType centerEvent, string name, string context, StationConfig config, bool sync = false)
        {
            if (Config.CanRaiseEvent != true)
                return;
            var args = new ZeroNetEventArgument(centerEvent, name, context, config);
            InvokeEvent(args, !sync);
        }

        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sync">是否为同步操作</param>
        internal static void RaiseEvent(ZeroNetEventType @event, bool sync = false)
        {
            if (Config.CanRaiseEvent != true)
                return;
            var args = new ZeroNetEventArgument(@event, null, null, null);
            InvokeEvent(args, !sync);
        }

        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="args"></param>
        /// <param name="waitEnd"></param>
        private static void InvokeEvent(object args, bool waitEnd)
        {
            var tasks = new List<Task>();
            var arg = (ZeroNetEventArgument)args;
            foreach (var action in ZeroNetEvents.ToArray())
            {
                try
                {
                    tasks.Add(action?.Invoke(Config, arg));
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("ZeroNetEvent", e, arg.Event);
                }
            }

            if (!waitEnd || tasks.Count == 0)
                return;
            Task.WaitAll(tasks.ToArray());
        }

        internal static List<ZmqRpcTransport> Transports = new List<ZmqRpcTransport>();
        /// <summary>
        ///     系统关闭时调用
        /// </summary>
        internal static Task OnZeroCenterClose(bool fromCenter = true)
        {
            foreach(var transfer in Transports)
            {
                transfer.Service.ConfigState = ZeroTeam.MessageMVC.StationStateType.Closed;
            }
            return Task.CompletedTask;
        }


        #endregion
    }
}