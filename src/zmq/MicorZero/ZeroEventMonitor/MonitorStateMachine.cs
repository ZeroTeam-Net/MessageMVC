using System;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ApiDocuments;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace ZeroTeam.ZeroMQ.ZeroRPC.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器状态机
    /// </summary>
    internal partial class MonitorStateMachine// : IDisposable
    {
        #region 状态变更

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
                    return;
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
                case ZeroCenterState.Destroy: // 已销毁，析构已调用
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

        static int ZeroMachineState;

        internal static Task center_start(string identity, string content)
        {
            if (ZeroMachineState == 1)
                return Task.CompletedTask;
            ZeroMachineState = 1;
            ZeroTrace.SystemLog("ZeroCenter", "center_start", $"{identity}:{ZeroRpcFlow.ZeroCenterState}:{ZeroMachineState}");
            if (ZeroRpcFlow.ZeroCenterState >= ZeroCenterState.Failed || ZeroRpcFlow.ZeroCenterState < ZeroCenterState.Start)
            {
                ZeroRpcFlow.JoinCenter();
            }
            else
            {
                ConfigManager.LoadAllConfig();
            }
            ZeroRpcFlow.RaiseEvent(ZeroNetEventType.CenterSystemStart, true);
            return Task.CompletedTask;
        }

        internal static async Task center_closing(string identity, string content)
        {
            if (ZeroMachineState >= 2)
                return;
            ZeroMachineState = 2;
            ZeroTrace.SystemLog("ZeroCenter", "center_closing", $"{identity}:{ZeroRpcFlow.ZeroCenterState}:{ZeroMachineState}");
            if (ZeroRpcFlow.ZeroCenterState < ZeroCenterState.Closing)
            {
                await center_closing();
            }
        }

        internal static async Task center_stop(string identity, string content)
        {
            if (ZeroMachineState == 3)
                return;
            ZeroMachineState = 3;
            ZeroTrace.SystemLog("ZeroCenter", "center_stop", $"{identity}:{ZeroRpcFlow.ZeroCenterState}:{ZeroMachineState}");
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
                return;
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
                ZeroTrace.WriteException(name, e, "station_state", content);
            }
        }
        internal static void station_update(string name, string content)
        {
            ZeroTrace.SystemLog(name, "station_update", content);
            if (ZeroRpcFlow.Config.UpdateConfig(ZeroRpcFlow.Config.Master, name, content, out var config))
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationUpdate, name, content, config);
            }
        }
        internal static void station_install(string name, string content)
        {
            ZeroTrace.SystemLog(name, "station_install", content);
            if (ZeroRpcFlow.Config.UpdateConfig(ZeroRpcFlow.Config.Master, name, content, out var config))
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationInstall, name, content, config);
            }
        }

        internal static void station_join(string name, string content)
        {
            ZeroTrace.SystemLog(name, "station_join", content);
            if (ZeroRpcFlow.Config.UpdateConfig(ZeroRpcFlow.Config.Master, name, content, out var config))
            {
                ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterStationJoin, name, content, config);
            }
        }
        internal static void ChangeStationState(string name, ZeroCenterState state, ZeroNetEventType eventType)
        {
            if (ZeroFlowControl.ApplicationState != StationRealState.Run || ZeroRpcFlow.ZeroCenterState != ZeroCenterState.Run)
                return;
            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config) || !config.ChangedState(state))
                return;
            ZeroRpcFlow.InvokeEvent(eventType, name, null, config);
        }


        internal static void station_uninstall(string name)
        {
            ZeroTrace.SystemLog(name, "station_uninstall");
            ChangeStationState(name, ZeroCenterState.Remove, ZeroNetEventType.CenterStationRemove);
        }


        internal static void station_closing(string name)
        {
            ZeroTrace.SystemLog(name, "station_closing");
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationClosing);
        }


        internal static void station_resume(string name)
        {
            ZeroTrace.SystemLog(name, "station_resume");
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationResume);
        }

        internal static void station_pause(string name)
        {
            ZeroTrace.SystemLog(name, "station_pause");
            ChangeStationState(name, ZeroCenterState.Pause, ZeroNetEventType.CenterStationPause);
        }

        internal static void station_left(string name)
        {
            ZeroTrace.SystemLog(name, "station_left");
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationLeft);
        }

        internal static void station_stop(string name)
        {
            ZeroTrace.SystemLog(name, "station_stop");
            ChangeStationState(name, ZeroCenterState.Stop, ZeroNetEventType.CenterStationStop);
        }



        internal static void station_document(string name, string content)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.SystemLog(name, "station_document");
            var doc = JsonConvert.DeserializeObject<ServiceDocument>(content);
            if (ZeroRpcFlow.Config.Documents.ContainsKey(name))
            {
                if (!ZeroRpcFlow.Config.Documents[name].IsLocal)
                    ZeroRpcFlow.Config.Documents[name] = doc;
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
                return;
            ZeroTrace.SystemLog(name, "client_join", content);
            ZeroRpcFlow.InvokeEvent(ZeroNetEventType.CenterClientJoin, name, content, config);
        }

        internal static void client_left(string name, string content)
        {
            if (!ZeroRpcFlow.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.SystemLog(name, "client_left", content);
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
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释