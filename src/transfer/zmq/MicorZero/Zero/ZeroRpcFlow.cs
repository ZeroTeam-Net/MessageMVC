using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     ZeroRPC流程控制器
    /// </summary>
    public partial class ZeroRpcFlow : IFlowMiddleware
    {
        #region 配置

        string IZeroMiddleware.Name => nameof(ZeroRpcFlow);

        /// <summary>
        /// 等级,用于确定中间件优先级
        /// </summary>
        int IZeroMiddleware.Level => -0xFFF;

        /// <summary>
        ///     站点配置
        /// </summary>
        public static ZeroRpcOption Config { get; set; }

        /// <summary>
        /// 工作模式
        /// </summary>
        public static ZeroWorkModel WorkModel { get; set; }

        internal static ILogger Logger;
        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppOption config)
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ZeroRpcFlow));
            ZContext.Initialize();

            #region 配置组合

            var sec = ConfigurationManager.Get("ZeroRPC");
            if (sec == null)
            {
                throw new Exception("无法找到主配置节点,路径为Zero,在zero.json或appsettings.json中设置");
            }

            var opt = sec.Child<SocketOption>("socketOption");
            if (opt != null)
            {
                ZSocket.Option = opt;
            }

            ZSocket.Option.CheckOption();
            Config = new ZeroRpcOption();
            var glc = sec.Child<ZeroRpcConfig>("Global");
            if (glc != null)
            {
                Config.CopyByEmpty(glc);
            }

            var cfg = sec.Child<ZeroRpcConfig>(ZeroAppOption.Instance.AppName);
            if (cfg != null)
            {
                Config.CopyByEmpty(cfg);
            }


            #endregion

            #region ServiceName

            if (string.IsNullOrWhiteSpace(Config.StationName))
            {
                Config.StationName = ZeroAppOption.Instance.AppName;
            }

            Config.ShortName = string.IsNullOrWhiteSpace(Config.ShortName)
                ? Config.StationName
                : Config.ShortName.Trim();

            if (string.IsNullOrWhiteSpace(Config.ServiceName))
            {
                Config.ServiceName = Config.StationName;
            }

            #endregion

            #region ZeroCenter

            ZeroCommandExtend.AppNameBytes = ZeroAppOption.Instance.AppName.ToBytes();

            Config.Master = Config.ZeroGroup[0];
            if (Config.ApiTimeout <= 1)
            {
                Config.ApiTimeout = 60;
            }
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
            var sec = ConfigurationManager.Get("ZeroRPC");
            var option = sec.Child<ZeroStationOption>(station) ?? new ZeroStationOption();
            option.CopyByEmpty(Config);
            if (option.ApiTimeout <= 1)
            {
                option.ApiTimeout = 60;
            }

            if (option.SpeedLimitModel != SpeedLimitType.WaitCount)
            {
                option.SpeedLimitModel = SpeedLimitType.Single;
            }

            //if (option.TaskCpuMultiple <= 0)
            //    option.TaskCpuMultiple = 1;
            //else if (option.TaskCpuMultiple > 128)
            //    option.TaskCpuMultiple = 128;

            if (option.MaxWait < 0xFF)
            {
                option.MaxWait = 0xFF;
            }
            else if (option.MaxWait > 0xFFFFF)
            {
                option.MaxWait = 0xFFFFF;
            }

            return option;
        }
        #endregion

        #region State

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
                Logger.Information(_state.ToString);
                if (IsAlive)
                {
                    MonitorStateMachine.SyncAppState();
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
        public static bool CanDo => ZeroFlowControl.IsRuning && ZerCenterIsRun;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ZeroFlowControl.ApplicationState < StationRealState.Destroy;

        /// <summary>
        /// 中心事件监控对象
        /// </summary>
        private static readonly ZeroEventMonitor SystemMonitor = new ZeroEventMonitor();

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            if (ZeroCenterState == ZeroCenterState.Start || ZeroCenterState == ZeroCenterState.Run)
            {
                return false;
            }
            Logger.Information(_state.ToString);

            ZeroCenterState = ZeroCenterState.Start;
            Logger.Information(() => $"Try connect zero center ({Config.Master.ManageAddress})...");
            if (!ZeroCenterProxy.Master.PingCenter())
            {
                SetFailed();
                Logger.Information("zero center can`t connection.");
                return false;
            }
            ZeroCenterState = ZeroCenterState.Run;
            if (!ZeroCenterProxy.Master.HeartJoin())
            {
                SetFailed();
                Logger.Information("zero center can`t join.");
                return false;
            }

            Config.ClearConfig();
            if (!StationConfigManager.LoadAllConfig())
            {
                SetFailed();
                Logger.Information("station configs can`t loaded.");
                return false;
            }
            Logger.Information("be connected successfully,start local stations.");

            if (ZeroFlowControl.ApplicationState == StationRealState.Run)
            {
                if (WorkModel == ZeroWorkModel.Service)
                {
                    var m = new StationConfigManager(Config.Master);
                    m.UploadDocument();
                }
            }
            return true;
        }

        #endregion

        #region Flow

        /// <summary>
        ///     初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            ZeroCenterProxy.Master = new ZeroCenterProxy(Config.Master);
            JoinCenter();
        }


        /// <summary>
        ///     启动
        /// </summary>
        void IFlowMiddleware.Start()
        {
            SystemMonitor.Start();
            if (ZeroFlowControl.ApplicationState == StationRealState.Run && WorkModel == ZeroWorkModel.Service)
            {
                var m = new StationConfigManager(Config.Master);
                m.UploadDocument();
            }
        }

        /// <summary>
        ///     关闭
        /// </summary>
        void IFlowMiddleware.End()
        {
            ZContext.Destroy();
        }

        #endregion

        #region Event

        /// <summary>
        /// 站点事件发生
        /// </summary>
        internal static List<Func<ZeroRpcOption, ZeroNetEventArgument, Task>> ZeroNetEvents = new List<Func<ZeroRpcOption, ZeroNetEventArgument, Task>>();

        /// <summary>
        /// 发出事件
        /// </summary>
        internal static void InvokeEvent(ZeroNetEventType centerEvent, string name, string context, StationConfig config, bool sync = false)
        {
            if (Config.CanRaiseEvent != true)
            {
                return;
            }

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
            {
                return;
            }

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
                    Logger.Exception(e, arg.Event.ToString());
                }
            }

            if (!waitEnd || tasks.Count == 0)
            {
                return;
            }

            Task.WaitAll(tasks.ToArray());
        }

        #endregion
    }
}