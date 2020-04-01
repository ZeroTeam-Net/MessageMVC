using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
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

        /// <summary>
        /// 构造
        /// </summary>
        public ZeroService()
        {
            InstanceName = GetType().Name;
            ConfigState = StationStateType.None;
        }

        int IFlowMiddleware.Level => short.MaxValue;

        /// <summary>
        /// 网络传输对象
        /// </summary>
        public INetTransfer Transport { get; set; }


        /// <summary>
        /// 网络传输对象构造器
        /// </summary>
        public Func<string, INetTransfer> TransportBuilder { get; set; }


        /// <summary>
        /// 站点名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string InstanceName { get; protected internal set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        public string RealName { get; protected set; }

        #endregion

        #region 状态管理

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
                ZeroTrace.SystemLog(ServiceName, nameof(RealState), StationState.Text(_realState));
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
            ZeroTrace.SystemLog(ServiceName, nameof(ConfigState), ConfigState, "StateMachine", StateMachine.GetTypeName());
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
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, int.MaxValue);

        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private Mutex mutex;

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Initialize()
        {
            if (InstanceName == null)
            {
                InstanceName = ServiceName;
            }

            RealState = StationState.Initialized;
            Transport = TransportBuilder(ServiceName);
            Transport.Service = this;
            Transport.Name = ServiceName;
            Transport.Initialize();
            ConfigState = ServiceName == null ? StationStateType.Stop : StationStateType.Initialized;
            ResetStateMachine();
            mutex = new Mutex();
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        private bool DoStart()
        {
            try
            {
                while (_waitToken.CurrentCount > 0)
                {
                    _waitToken.Wait();
                }

                if (ConfigState == StationStateType.None || ConfigState >= StationStateType.Stop || !ZeroFlowControl.CanDo)
                {
                    return false;
                }

                RealState = StationState.Start;
                //名称初始化
                RealName = $"{ZeroFlowControl.Config.ServiceName}-{RandomOperate.Generate(6)}";
                ZeroTrace.SystemLog(ServiceName, InstanceName, RealName);
                //扩展动作
                if (!Transport.Prepare())
                {
                    RealState = StationState.Failed;
                    ConfigState = StationStateType.Failed;
                    return false;
                }

                ConfigState = StationStateType.Run;
                //可执行
                //Hearter.HeartJoin(Config.StationName, RealName);
                //执行主任务
                CancelToken = new CancellationTokenSource();
                Task.Factory.StartNew(Run, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning);
                //保证Run真正执行后再完成本方法调用.
                _waitToken.Wait();
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
            if (!LoopBegin())
            {
                ResetStateMachine();
                _waitToken.Release();
                return;
            }
            RealState = StationState.Run;

            ResetStateMachine();
            _waitToken.Release();
            mutex.WaitOne();
            {
                try
                {
                    success = await Transport.Loop(CancelToken.Token);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(ServiceName, e, "Run");
                    RealState = StationState.Failed;
                    success = false;
                }
                finally
                {
                    LoopComplete();
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
                        Thread.Sleep(100);
                        _ = Task.Factory.StartNew(StateMachine.Start, TaskCreationOptions.DenyChildAttach);
                        return;
                    }
                }
                ResetStateMachine();
            }
            mutex.ReleaseMutex();
            GC.Collect();
        }


        #endregion

        #region 扩展流程

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        private bool LoopBegin()
        {
            if (ConfigState != StationStateType.Run)
            {
                ZeroFlowControl.OnObjectFailed(this);
                return false;
            }
            if (!Transport.LoopBegin())
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
        private void LoopComplete()
        {
            Transport.LoopComplete();
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
        protected internal virtual void OnLoopIdle()
        {
            Thread.Sleep(200);
        }

        /// <summary>
        /// 析构
        /// </summary>
        protected virtual void DoEnd()
        {
            Transport.Dispose();
            mutex.Dispose();
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

        bool IStateMachineControl.DoStart()
        {
            if (RealState == StationState.BeginRun || RealState == StationState.Run)
            {
                return true;//已启动,不应该再次
            }

            mutex.WaitOne();
            try
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
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        bool IStateMachineControl.DoClose()
        {
            if (RealState >= StationState.Closing || CancelToken == null || CancelToken.IsCancellationRequested)
            {
                ZeroTrace.WriteError(ServiceName, "Close", "station not runing");
                return false;
            }
            RealState = StationState.Closing;
            CancelToken.Cancel();
            Transport.Close();
            ZeroTrace.SystemLog(ServiceName, "Close", "Run is cancel,waiting... ");
            return true;
        }

        /// <summary>
        /// 析构
        /// </summary>
        void IStateMachineControl.DoEnd()
        {
            DoEnd();
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

            ZeroTrace.SystemLog(ServiceName,
                info == null
                    ? name
                    : info.Controller == null
                        ? $"{name}({info.Name})"
                        : $"{name}({info.Controller}.{info.Name})");
        }

        #endregion
    }
}