using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Documents;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable IDE1006 // 命名样式
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
namespace ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器状态机
    /// </summary>
    internal partial class MonitorStateMachine// : IDisposable
    {
        #region 状态变更

        internal static ILogger Logger = DependencyHelper.LoggerFactory.CreateLogger("ZeroManagemant");

        private static IMonitorStateMachine _stateMachine = new EmptyStateMachine();

        /// <summary>
        /// 状态机
        /// </summary>
        public static IMonitorStateMachine StateMachine
        {
            get => _stateMachine;
            internal set
            {
                if (_stateMachine.GetType() == value.GetType())
                {
                    return;
                }
                //_stateMachine?.Dispose();
                _stateMachine = value;
            }
        }

        /// <summary>
        /// 状态变更同步状态机
        /// </summary>
        public static void SyncAppState()
        {
            switch (ZeroRpcFlow.ZeroCenterState)
            {
                case ZeroCenterState.None: // 刚构造
                case ZeroCenterState.Start: // 正在启动
                case ZeroCenterState.Closing: // 将要关闭
                case ZeroCenterState.Closed: // 已关闭
                case ZeroCenterState.Destroy: // 已注销，析构已调用
                                              //StateMachine = new EmptyStateMachine();
                                              //return;
                case ZeroCenterState.Failed: // 错误状态
                    ZeroMachineState = 3;
                    StateMachine = new FailedStateMachine();
                    return;
                case ZeroCenterState.Run: // 正在运行
                    ZeroMachineState = 1;
                    StateMachine = new RuningStateMachine();
                    return;
            }
        }

        #endregion

        #region CenterEvent

        private static int ZeroMachineState;

        internal static Task center_start(string identity)
        {
            if (ZeroMachineState == 1)
            {
                return Task.CompletedTask;
            }

            ZeroMachineState = 1;
            Logger.Information("ZeroCenter event: center_start.");
            if (ZeroRpcFlow.ZeroCenterState >= ZeroCenterState.Failed ||
                ZeroRpcFlow.ZeroCenterState < ZeroCenterState.Start)
            {
                ZeroRpcFlow.JoinCenter();
            }
            else
            {
                StationConfigManager.LoadAllConfig();
            }
            ZeroRpcFlow.RaiseEvent(ZeroNetEventType.CenterSystemStart, true);
            return Task.CompletedTask;
        }

        internal static async Task center_closing(string identity)
        {
            if (ZeroMachineState >= 2)
            {
                return;
            }

            ZeroMachineState = 2;
            Logger.Information("ZeroCenter event: center_closing");
            if (ZeroRpcFlow.ZeroCenterState < ZeroCenterState.Closing)
            {
                await center_closing();
            }
        }

        internal static async Task center_stop(string identity)
        {
            if (ZeroMachineState == 3)
            {
                return;
            }

            ZeroMachineState = 3;
            Logger.Information("ZeroCenter event: center_stop");
            if (ZeroRpcFlow.ZeroCenterState < ZeroCenterState.Closing)
            {
                await center_stop();
            }
        }

        internal static async Task center_closing()
        {
            ZeroRpcFlow.ZeroCenterState = ZeroCenterState.Closing;
            ZeroRpcFlow.RaiseEvent(ZeroNetEventType.CenterSystemClosing);
        }
        internal static async Task center_stop()
        {
            ZeroRpcFlow.ZeroCenterState = ZeroCenterState.Closed;
            ZeroRpcFlow.RaiseEvent(ZeroNetEventType.CenterSystemStop);
        }
        #endregion

        #region StationEvent

        /// <summary>
        /// 站点心跳
        /// </summary>
        internal static void worker_sound_off()
        {
            if (ZeroFlowControl.ApplicationState != StationRealState.Run || ZeroRpcFlow.ZeroCenterState != ZeroCenterState.Run)
            {
                return;
            }

            ZeroCenterProxy.Master.Heartbeat();
        }

        /// <summary>
        /// 站点动态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        internal static void station_trends(string name, string content)
        {
            try
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationTrends, name, content, null);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
        internal static void station_update(string name, string content)
        {
            Logger.Information("ZeroCenter event: station_update({0})", name);

            if (ZeroRpcFlow.Config.UpdateConfig(ZeroRpcFlow.Config.Master, name, content, out var config))
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationUpdate, name, content, config);
            }
        }
        internal static void station_install(string name, string content)
        {
            Logger.Information("ZeroCenter event: station_install({0})", name);
            if (ZeroRpcFlow.Config.UpdateConfig(ZeroRpcFlow.Config.Master, name, content, out var config))
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationInstall, name, content, config);
            }
        }

        internal static void station_join(string name, string content)
        {
            Logger.Information("ZeroCenter event: station_join({0})", name);
            if (ZeroRpcFlow.Config.UpdateConfig(ZeroRpcFlow.Config.Master, name, content, out var config))
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationJoin, name, content, config);
            }
        }
        internal static void ChangeStationState(string name, ZeroCenterState state, ZeroNetEventType eventType)
        {
            if (ZeroFlowControl.ApplicationState != StationRealState.Run || ZeroRpcFlow.ZeroCenterState != ZeroCenterState.Run)
            {
                return;
            }

            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config) || !config.ChangedState(state))
            {
                return;
            }

            ZeroRpcFlow.InvokeEvent(eventType, name, null, config);
        }


        internal static void station_uninstall(string name)
        {
            Logger.Information("ZeroCenter event: station_uninstall({0})", name);
            ChangeStationState(name, ZeroCenterState.Remove, ZeroNetEventType.CenterStationRemove);
        }


        internal static void station_closing(string name)
        {
            Logger.Information("ZeroCenter event: station_closing({0})", name);
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationClosing);
        }


        internal static void station_resume(string name)
        {
            Logger.Information("ZeroCenter event: station_resume({0})", name);
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationResume);
        }

        internal static void station_pause(string name)
        {
            Logger.Information("ZeroCenter event: station_pause({0})", name);
            ChangeStationState(name, ZeroCenterState.Pause, ZeroNetEventType.CenterStationPause);
        }

        internal static void station_left(string name)
        {
            Logger.Information("ZeroCenter event: station_left({0})", name);
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationLeft);
        }

        internal static void station_stop(string name)
        {
            Logger.Information("ZeroCenter event: station_stop({0})", name);
            ChangeStationState(name, ZeroCenterState.Stop, ZeroNetEventType.CenterStationStop);
        }



        internal static void station_document(string name, string content)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config))
            {
                return;
            }

            Logger.Information("ZeroCenter event: station_document({0})", name);
            var doc = JsonConvert.DeserializeObject<ServiceDocument>(content);
            if (ZeroRpcFlow.Config.Documents.ContainsKey(name))
            {
                if (!ZeroRpcFlow.Config.Documents[name].IsLocal)
                {
                    ZeroRpcFlow.Config.Documents[name] = doc;
                }
            }
            else
            {
                ZeroRpcFlow.Config.Documents.Add(name, doc);
            }
            ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationDocument, name, content, config);
        }



        #endregion

        #region ClientEvent



        internal static void client_join(string name, string content)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config))
            {
                return;
            }
            ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterClientJoin, name, content, config);
        }

        internal static void client_left(string name, string content)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config))
            {
                return;
            }
            ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterClientLeft, name, content, config);
        }

        #endregion

        #region IDisposable
        /*
        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; internal set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }
        */
        #endregion
    }
}
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
#pragma warning restore IDE1006 // 命名样式
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释