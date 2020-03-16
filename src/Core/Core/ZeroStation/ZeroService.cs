using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
using Agebull.MicroZero.ApiDocuments;
using Agebull.MicroZero.ZeroApis;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC.ZeroServices.StateMachine;


namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 一个服务
    /// </summary>
    public class ZeroService : IService
    {

        #region 基础信息

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="type">站点类型</param>
        public ZeroService(ZeroStationType type)
        {
            StationType = type;
            InstanceName = GetType().Name;
            ConfigState = StationStateType.None;
        }

        /// <summary>
        /// 网络传输对象
        /// </summary>
        public INetTransport NetPool { get; protected set; }


        /// <summary>
        /// 网络传输对象构造器
        /// </summary>
        public Func<string, INetTransport> NetPoolBuilder { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public ZeroStationType StationType { get; }
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
                ZeroTrace.SystemLog(ServiceName, nameof(RealState), StationState.Text(_realState));
            }
        }
        //#if UseStateMachine
        /// <summary>
        /// 状态机
        /// </summary>
        protected IStationStateMachine StateMachine { get; private set; }
        //#endif

        private StationStateType _configState;

        /// <summary>
        ///     配置状态
        /// </summary>
        public StationStateType ConfigState
        {
            get => _configState;
            set
            {
                if (_configState == value)
                    return;
                //#if UseStateMachine
                switch (value)
                {
                    case StationStateType.None:
                    case StationStateType.Stop:
                    case StationStateType.Remove:
                        if (!(StateMachine is EmptyStateMachine) || StateMachine.IsDisposed)
                            StateMachine = new EmptyStateMachine { Station = this };
                        break;
                    case StationStateType.Run:
                        if (ZeroApplication.CanDo)
                            StateMachine = new RunStateMachine { Station = this };
                        else
                            StateMachine = new StartStateMachine { Station = this };
                        break;
                    case StationStateType.Failed:
                        if (ZeroApplication.CanDo)
                            StateMachine = new FailedStateMachine { Station = this };
                        else
                            StateMachine = new StartStateMachine { Station = this };
                        break;
                    default:
                        StateMachine = new StartStateMachine { Station = this };
                        break;
                }
                //#endif
                _configState = value;
                //#if UseStateMachine
                ZeroTrace.SystemLog(ServiceName, nameof(ConfigState), value, "StateMachine", StateMachine.GetTypeName());
                //#else
                //                ZeroTrace.SystemLog(StationName, nameof(ConfigState), value);
                //#endif
            }
        }


        /// <summary>
        /// 取消标记
        /// </summary>
        protected CancellationTokenSource CancelToken { get; set; }

        /// <summary>
        /// 能不能循环处理
        /// </summary>
        protected internal bool CanLoop => ZeroApplication.CanDo &&
                                  ConfigState == StationStateType.Run &&
                                  (RealState == StationState.BeginRun || RealState == StationState.Run) &&
                                  CancelToken != null && !CancelToken.IsCancellationRequested;


        #endregion

        #region 主流程

        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, int.MaxValue);

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Initialize()
        {
            if (InstanceName == null)
                InstanceName = ServiceName;
            RealState = StationState.Initialized;
            NetPool = NetPoolBuilder(ServiceName);
            NetPool.Service = this;
            NetPool.Name = ServiceName;
            NetPool.Initialize();
            ConfigState = ServiceName == null ? StationStateType.ConfigError : StationStateType.Initialized;
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (DoStart())
                return true;
            RealState = StationState.Failed;
            ZeroApplication.OnObjectFailed(this);
            return false;
        }

        private readonly LockData _startLock = new LockData();
        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        private bool DoStart()
        {
            try
            {
                using (var scope = OnceScope.TryCreateScope(_startLock))
                {
                    if (!scope.IsEntry)
                        return false;
                    while (_waitToken.CurrentCount > 0)
                        _waitToken.Wait();
                    if (ConfigState == StationStateType.None || ConfigState >= StationStateType.Stop || !ZeroApplication.CanDo)
                        return false;
                    ConfigState = StationStateType.Run;

                    RealState = StationState.Start;
                    //名称初始化
                    RealName = $"{ZeroApplication.Config.StationName}-{RandomOperate.Generate(6)}";
                    ZeroTrace.SystemLog(ServiceName, InstanceName, StationType, RealName);
                    //扩展动作
                    if (!NetPool.Prepare())
                    {
                        return false;
                    }
                }
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
        private void Run()
        {
            bool success;
            LoopBegin();
            try
            {
                RealState = StationState.Run;
                success = NetPool.Loop(CancelToken.Token);
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
                ConfigState = !ZeroApplication.CanDo || success ? StationStateType.Closed : StationStateType.Failed;
            GC.Collect();
            //#if UseStateMachine
            StateMachine.End();
            //#endif
        }


        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (CancelToken == null || CancelToken.IsCancellationRequested)
            {
                ZeroTrace.WriteError(ServiceName, "Close", "station not runing");
                return false;
            }
            RealState = StationState.Closing;
            CancelToken.Cancel();
            ZeroTrace.SystemLog(ServiceName, "Close", "Run is cancel,waiting... ");
            return true;
        }
        #endregion

        #region 扩展流程

        /// <summary>
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        private void LoopBegin()
        {
            RealState = StationState.BeginRun;
            if (ConfigState == StationStateType.Run)
            {
                ZeroApplication.OnObjectActive(this);
            }
            else
            {
                ZeroApplication.OnObjectFailed(this);
            }
            NetPool.LoopBegin();
            _waitToken.Release();
        }

        /// <summary>
        /// 同步关闭状态
        /// </summary>
        /// <returns></returns>
        private void LoopComplete()
        {
            if (CancelToken == null)
                return;
            NetPool.LoopComplete();
            CancelToken.Dispose();
            CancelToken = null;
            RealState = StationState.Closed;
            ZeroApplication.OnObjectClose(this);

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
        protected virtual void DoDispose()
        {
            NetPool.Dispose();
        }
        /// <summary>
        /// 析构
        /// </summary>
        protected virtual void DoDestory()
        {

        }


        #endregion

        #region IZeroObject


        void IService.OnInitialize()
        {
            Initialize();
        }

        bool IService.OnStart()
        {
            return StateMachine.Start();
        }

        bool IService.OnEnd()
        {
            //#if UseStateMachine
            return StateMachine.Close();
            //#else
            //            return Close();
            //#endif
        }

        void IService.OnDestory()
        {
            DoDestory();
        }

        private bool _isDisposed;

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            DoDispose();
        }
        #endregion

        #region 方法注册

        internal readonly Dictionary<string, ApiAction> ApiActions = new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        public void RegistAction(string name, ApiAction action, ApiActionInfo info = null)
        {
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

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<Task<IApiResult>> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiTaskAction<IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<object, Task<IApiResult>> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiTaskAction2<IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<IApiResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction<IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction(string name, Func<object, object> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiActionObj
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction(string name, Func<object> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiActionObj2
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction<TResult>(string name, Func<TResult> action, ApiAccessOption access, ApiActionInfo info = null)
            where TResult : IApiResult
        {
            var a = new ApiAction<TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }
        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction2<TResult>(string name, Func<TResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction2<TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction2<TArg, TResult>(string name, Func<TArg, TResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction2<TArg, TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction(string name, Func<IApiArgument, IApiResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction<IApiArgument, IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);

            return a;
        }
        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction<TArgument, TResult>(string name, Func<TArgument, TResult> action, ApiAccessOption access, ApiActionInfo info = null)
            where TArgument : class, IApiArgument
            where TResult : IApiResult
        {
            var a = new ApiAction<TArgument, TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);

            return a;
        }

        #endregion

        #region 方法扩展

        /// <summary>
        /// 处理器
        /// </summary>
        private static readonly List<Func<IApiHandler>> ApiHandlers = new List<Func<IApiHandler>>();

        /// <summary>
        ///     注册处理器
        /// </summary>
        public static void RegistHandlers<TApiHandler>() where TApiHandler : class, IApiHandler, new()
        {
            ApiHandlers.Add(() => new TApiHandler());
        }

        private List<IApiHandler> _handlers;

        /// <summary>
        ///     注册处理器
        /// </summary>
        internal List<IApiHandler> CreateHandlers()
        {
            if (ApiHandlers.Count == 0)
                return null;
            if (_handlers != null)
                return _handlers;
            _handlers = new List<IApiHandler>();
            foreach (var func in ApiHandlers)
                _handlers.Add(func());
            return _handlers;
        }

        #endregion
    }
}