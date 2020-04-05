using Agebull.Common.Frame;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ApiDocuments;
using ZeroTeam.MessageMVC.ZeroApis.StateMachine;


namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// 一个服务
    /// </summary>
    public class ZeroService : IService, IStateMachineControl
    {
        #region 基础信息

        string IZeroMiddleware.Name => ServiceName;

        /// <summary>
        /// 等级,用于确定中间件优先级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 站点名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 网络传输对象
        /// </summary>
        public INetTransfer Transport { get; set; }

        /// <summary>
        /// 是否自动发现对象
        /// </summary>
        public bool IsDiscover { get; internal set; }

        /// <summary>
        /// 网络传输对象构造器
        /// </summary>
        public Func<string, INetTransfer> TransportBuilder { get; set; }

        #endregion

        #region 状态管理

        ILogger logger;

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
                logger.Information(()=>$"[RealState] {StationState.Text(_realState)}");
            }
        }
        //#if UseStateMachine
        /// <summary>
        /// 状态机
        /// </summary>
        protected IStationStateMachine StateMachine { get; private set; }

        /// <summary>
        ///     配置状态
        /// </summary>
        public StationStateType ConfigState { get; set; }

        /// <summary>
        /// 重置状态机,请谨慎使用
        /// </summary>
        public void ResetStateMachine()
        {
            switch (ConfigState)
            {
                case StationStateType.None:
                case StationStateType.Stop:
                case StationStateType.Remove:
                    StateMachine = new EmptyStateMachine { Service = this };
                    break;
                case StationStateType.Run:
                    StateMachine = new RunStateMachine { Service = this };
                    break;
                case StationStateType.Closed:
                    StateMachine = new CloseStateMachine { Service = this };
                    break;
                default:
                    StateMachine = new StartStateMachine { Service = this };
                    break;
            }
            logger.Information(() => $"[ConfigState] {ConfigState} [StateMachine] {StateMachine.GetTypeName()}");
        }

        /// <summary>
        /// 取消标记
        /// </summary>
        protected CancellationTokenSource CancelToken { get; set; }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        protected internal bool CanLoop => ZeroFlowControl.CanDo &&
                                  ConfigState == StationStateType.Run &&
                                  (RealState == StationState.BeginRun || RealState == StationState.Run) &&
                                  CancelToken != null && !CancelToken.IsCancellationRequested;


        #endregion

        #region 执行流程

        /// <summary>
        /// Run与Start互斥执行的事件对象
        /// </summary>
        private ManualResetEventSlim eventSlim;

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Initialize()
        {
            logger ??= IocHelper.LoggerFactory.CreateLogger($"ZeroService({ServiceName})");

            eventSlim = new ManualResetEventSlim(true);
            if (ServiceName == null)
            {
                ConfigState = StationStateType.ConfigError;
            }
            else
            {
                RealState = StationState.Initialized;
                Transport = TransportBuilder(ServiceName);
                Transport.Service = this;
                Transport.Initialize();
                if (!Transport.Prepare())
                    ConfigState = StationStateType.ConfigError;
                else
                    ConfigState = StationStateType.Initialized;
            }
            ResetStateMachine();
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        private bool DoStart()
        {
            logger.Information(()=> $"Try start by {StationState.Text(RealState)}");
            try
            {
                if (ConfigState == StationStateType.None || ConfigState >= StationStateType.Stop || !ZeroFlowControl.CanDo)
                {
                    logger.Warning(() => $"Start failed. ConfigState :{ConfigState} ,ZeroFlowControl.CanDo {ZeroFlowControl.CanDo}");
                    return false;
                }
                RealState = StationState.Start;
                ConfigState = StationStateType.Run;
                //可执行
                //Hearter.HeartJoin(Config.StationName, RealName);
                //执行主任务
                CancelToken = new CancellationTokenSource();
                _ = Task.Factory.StartNew(Run, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning);
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return false;
            }
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private async void Run()
        {
            bool success;
            RealState = StationState.BeginRun;
            if (!await LoopBegin())
            {
                ResetStateMachine();
                return;
            }
            RealState = StationState.Run;

            ResetStateMachine();
            logger.Information("[Run]");
            using (ManualResetEventSlimScope.Scope(eventSlim))
            {
                try
                {
                    success = await Transport.Loop(CancelToken.Token);
                }
                catch (TaskCanceledException)
                {
                    success = true;
                }
                catch (Exception e)
                {
                    logger.Exception(e, "Function : Run");
                    RealState = StationState.Failed;
                    success = false;
                }
                finally
                {
                    await LoopComplete();
                }

                if (ConfigState < StationStateType.Stop)
                {
                    if (!ZeroFlowControl.CanDo)
                    {
                        ConfigState = StationStateType.Stop;
                    }
                    else if (success)
                    {
                        ConfigState = StationStateType.Closed;
                    }
                    else
                    {
                        ConfigState = StationStateType.Failed;
                        ResetStateMachine();
                        await Task.Delay(100);
                        _ = Task.Factory.StartNew(StateMachine.Start);
                        return;
                    }
                }
                ResetStateMachine();
            }
            GC.Collect();
        }


        #endregion

        #region 扩展流程

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        private async Task<bool> LoopBegin()
        {
            if (ConfigState != StationStateType.Run)
            {
                ZeroFlowControl.OnObjectFailed(this);
                return false;
            }
            if (!await Transport.LoopBegin())
            {
                RealState = StationState.Failed;
                ConfigState = StationStateType.Failed;
                ZeroFlowControl.OnObjectFailed(this);
                return false;
            }
            ZeroFlowControl.OnObjectActive(this);
            return true;
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        private async Task LoopComplete()
        {
            await Transport.LoopComplete();
            CancelToken.Dispose();
            CancelToken = null;
            if (ConfigState == StationStateType.Failed)
            {
                ZeroFlowControl.OnObjectFailed(this);
            }
            else
            {
                ZeroFlowControl.OnObjectClose(this);
            }
            RealState = StationState.Closed;
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        public void OnLoopIdle()
        {

        }

        /// <summary>
        /// 析构
        /// </summary>
        void DoEnd()
        {
            Transport.End();
            eventSlim.Dispose();
        }
        #endregion

        #region IAppMiddleware


        void IFlowMiddleware.Initialize()
        {
            Initialize();
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        void IFlowMiddleware.Start()
        {
            StateMachine.Start();
        }

        void IFlowMiddleware.Close()
        {
            StateMachine.Close();
        }

        private bool _isDisposed;
        /// <inheritdoc/>
        void IFlowMiddleware.End()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            StateMachine.End();
        }
        #endregion


        #region 状态机接口

        async Task<bool> IStateMachineControl.DoStart()
        {
            if (RealState == StationState.BeginRun || RealState == StationState.Run)
            {
                return true;//已启动,不应该再次
            }

            using (ManualResetEventSlimScope.Scope(eventSlim))
            {
                if (RealState == StationState.BeginRun || RealState == StationState.Run)
                {
                    return true;//已启动,不应该再次
                }

                if (DoStart())
                {
                    return true;
                }

                RealState = StationState.Failed;

                ResetStateMachine();
                ZeroFlowControl.OnObjectFailed(this);
                return false;
            }
        }

        async Task<bool> IStateMachineControl.DoClose()
        {
            if (RealState >= StationState.Closing || CancelToken == null || CancelToken.IsCancellationRequested)
            {
                logger.Warning("[Close] service is closed");
                return false;
            }
            RealState = StationState.Closing;
            CancelToken.Cancel();
            await Transport.Close();
            logger.Information("[Close] cancel,waiting loop end... ");
            return true;
        }

        /// <summary>
        /// 析构
        /// </summary>
        Task<bool> IStateMachineControl.DoEnd()
        {
            DoEnd();
            return Task.FromResult(true);
        }

        #endregion


        #region 方法注册


        /// <summary>
        /// 注册的方法
        /// </summary>
        Dictionary<string, IApiAction> IService.Actions => ApiActions;

        internal readonly Dictionary<string, IApiAction> ApiActions = new Dictionary<string, IApiAction>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        public void RegistAction(string name, ApiAction action, ApiActionInfo info = null)
        {
            if (info != null && info.HaseArgument && action.ArgumentType != null)
            {
                action.ArgumentType = info.ArgumentType;
            }

            if (info != null && action.ResultType != null)
            {
                action.ResultType = info.ResultType;
            }

            action.Initialize();
            if (!ApiActions.ContainsKey(name))
            {
                action.Name = name;
                ApiActions.Add(name, action);
            }
            else
            {
                ApiActions[name] = action;
            }
            logger ??= IocHelper.LoggerFactory.CreateLogger($"ZeroService({ServiceName})");
            logger.Information(() => $"[Regist Action] {name}({info.Controller}.{info.Name})");
        }

        #endregion
    }
}