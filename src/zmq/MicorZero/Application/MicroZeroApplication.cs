using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agebull.MicroZero.ZeroManagemant;
using ZeroMQ;
using ZeroTeam.MessageMVC;
using Agebull.Common.Configuration;

namespace Agebull.MicroZero
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
            if (string.IsNullOrWhiteSpace(Config.StationName))
                Config.StationName = AppName;


            #endregion

            #region ServiceName

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
            //if (WorkModel == ZeroWorkModel.Bridge)
            //    return;

            //if (string.IsNullOrWhiteSpace(Config.ZeroAddress))
            //    Config.ZeroAddress = "127.0.0.1";

            //if (Config.ZeroManagePort <= 1024 || Config.ZeroManagePort >= 65000)
            //    Config.ZeroManagePort = 8000;

            //if (Config.ZeroMonitorPort <= 1024 || Config.ZeroMonitorPort >= 65000)
            //    Config.ZeroMonitorPort = 8001;

            //if (Config.PoolSize > 4096 || Config.PoolSize < 10)
            //    Config.PoolSize = 100;

            //Config.ZeroManageAddress = $"tcp://{Config.Master.Address}:{Config.Master.ManagePort}";
            //Config.ZeroMonitorAddress = $"tcp://{Config.Master.Address}:{Config.Master.MonitorPort}";

            #endregion

            ZeroCommandExtend.AppNameBytes = AppName.ToZeroBytes();
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
        ///     应用中心状态
        /// </summary>
        public static ZeroCenterState ZeroCenterState { get; internal set; }

        /// <summary>
        ///     状态
        /// </summary>
        public static int ApplicationState
        {
            get => ZeroFlowControl.ApplicationState;
            internal set
            {
                ZeroFlowControl.ApplicationState = value;
            }
        }

        /// <summary>
        /// 设置ZeroCenter与Application状态都为Failed
        /// </summary>
        public static void SetFailed()
        {
            ZeroCenterState = ZeroCenterState.Failed;
            ApplicationState = StationState.Failed;
        }

        /// <summary>
        ///     应用中心是否正在运行
        /// </summary>
        public static bool ZerCenterIsRun => ZeroCenterState == ZeroCenterState.Run;

        /// <summary>
        ///     本地应用是否正在运行
        /// </summary>
        public static bool ApplicationIsRun => ApplicationState == StationState.BeginRun || ApplicationState == StationState.Run;

        /// <summary>
        ///     运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo => ApplicationIsRun && ZerCenterIsRun;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ApplicationState < StationState.Destroy;

        /// <summary>
        ///     已关闭
        /// </summary>
        public static bool IsDisposed => ApplicationState == StationState.Disposed;

        /// <summary>
        ///     已关闭
        /// </summary>
        public static bool IsClosed => ApplicationState >= StationState.Closed;

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

            ZeroCenterState = ZeroCenterState.None;
            JoinCenter();
        }


        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            if (ApplicationIsRun)
                return false;
            ZeroCenterState = ZeroCenterState.Start;
            ApplicationState = StationState.BeginRun;
            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", $"try connect zero center ({Config.Master.ManageAddress})...");
            if (!ZeroCenterProxy.Master.PingCenter())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "zero center can`t connection.");
                return false;
            }
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
            ZeroCenterState = ZeroCenterState.Run;

            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", "be connected successfully,start local stations.");

            return true;
        }

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        public void Start()
        {
            SystemMonitor.Start();
            ApplicationState = StationState.Start;
            if (ApplicationState == StationState.Run && WorkModel == ZeroWorkModel.Service)
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

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            //foreach (var obj in ZeroObjects.Values.Where(p => p.StationName == config.StationName).ToArray())
            //{
            //    try
            //    {
            //        ZeroTrace.SystemLog(obj.Name, "[OnStationStateChanged>>", config.State);
            //        obj.OnStationStateChanged(config);
            //        ZeroTrace.SystemLog(obj.Name, "<<OnStationStateChanged]");
            //    }
            //    catch (Exception e)
            //    {
            //        ZeroTrace.WriteException(obj.StationName, e, "OnStationStateChanged");
            //    }
            //}
        }

        /// <summary>
        ///     系统关闭时调用
        /// </summary>
        internal static async Task OnZeroCenterClose(bool fromCenter = true)
        {
            if (ApplicationState >= StationState.Closing || ApplicationState == StationState.Failed)
            {
                return;
            }
            ZeroTrace.SystemLog(StationState.Text(ApplicationState), "[OnZeroEnd>>");
            ApplicationState = StationState.Closing;

            //RaiseEvent(ZeroNetEventType.AppStop, true);
            ////using (OnceScope.CreateScope(ZeroObjects))
            //{
            //    IZeroObject[] array;
            //    lock (ActiveObjects)
            //        array = ActiveObjects.ToArray();
            //    foreach (var obj in array)
            //    {
            //        try
            //        {
            //            ZeroTrace.SystemLog("OnZeroEnd", obj.StationName);
            //            obj.OnZeroEnd();
            //        }
            //        catch (Exception e)
            //        {
            //            ZeroTrace.WriteException("OnZeroEnd", e, obj.StationName);
            //        }
            //    }
            //    await WaitAllObjectSafeClose();
            //}
            //ApplicationState = /*fromCenter ? StationState.Failed :*/ StationState.Closed;
            GC.Collect();
            ZeroTrace.SystemLog(StationState.Text(ApplicationState), "<<OnZeroEnd]");
        }

        #endregion
    }
}