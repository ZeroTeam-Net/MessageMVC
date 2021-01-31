using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services.StateMachine;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Services
{

    /// <summary>
    /// 一个服务
    /// </summary>
    public class ZeroService : IService, IStateMachineControl
    {
        #region 基础信息

        string IZeroDependency.Name => ServiceName;

        /// <summary>
        ///     是否可以运行的判断方法
        /// </summary>
        public Func<bool> CanRun { get; set; }

        /// <summary>
        /// 等级,用于确定中间件优先级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.General;

        /// <summary>
        /// 站点名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 消息接收对象
        /// </summary>
        public IMessageReceiver Receiver { get; set; }

        /// <summary>
        /// 是否自动发现对象
        /// </summary>
        public bool IsAutoService { get; set; }

        #endregion

        #region 状态管理

        private ILogger logger;

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
                logger.Information(() => $"[RealState] {StationState.Text(_realState)}");
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
        void ResetStateMachine()
        {
            switch (ConfigState)
            {
                case StationStateType.Initialized:
                    StateMachine = new StartStateMachine { Service = this };
                    break;
                case StationStateType.Run:
                    StateMachine = new RunStateMachine { Service = this };
                    break;
                case StationStateType.Closed:
                    StateMachine = new CloseStateMachine { Service = this };
                    break;
                default:
                    StateMachine = new EmptyStateMachine { Service = this };
                    break;
            }
            logger.Information(() => $"[ConfigState] {ConfigState} [StateMachine] {StateMachine.GetTypeName()}");
        }
        /// <summary>
        /// 取消标记
        /// </summary>
        protected CancellationTokenSource CancelToken { get; set; }

        #endregion

        #region 执行流程

        /// <summary>
        /// Run与Start互斥执行的事件对象
        /// </summary>
        private ManualResetEventSlim eventSlim;

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        private bool DoStart()
        {
            logger.Information(() => $"Try start by {StationState.Text(RealState)}");
            try
            {
                if (ConfigState == StationStateType.None || ConfigState >= StationStateType.Stop || !ZeroAppOption.Instance.IsRuning)
                {
                    logger.Warning(() => $"Start failed. ConfigState :{ConfigState} ,ZeroFlowControl.CanDo {ZeroAppOption.Instance.IsRuning}");
                    return false;
                }
                RealState = StationState.Start;
                //可执行
                //Hearter.HeartJoin(Config.StationName, RealName);
                //执行主任务
                ConfigState = StationStateType.Run;
                _ = Task.Run(Run);
                return true;
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return false;
            }
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private async Task Run()
        {
            bool success;
            RealState = StationState.BeginRun;
            if (!await LoopBegin())
            {
                return;
            }
            RealState = StationState.Run;
            ResetStateMachine();
            logger.Information("[Run]");
            CancelToken = new CancellationTokenSource();
            using (ManualResetEventSlimScope.Scope(eventSlim))
            {
                try
                {
                    success = await Receiver.Loop(CancelToken.Token);
                }
                catch (OperationCanceledException)
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

                CancelToken = null;
                if (ConfigState < StationStateType.Stop)
                {
                    if (!ZeroAppOption.Instance.IsRuning)
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
                RealState = StationState.Failed;
                ConfigState = StationStateType.ConfigError;
                ResetStateMachine();
                return false;
            }
            if (!await Receiver.LoopBegin())
            {
                ZeroFlowControl.OnObjectFailed(this);
                RealState = StationState.Failed;
                ConfigState = StationStateType.Failed;
                ResetStateMachine();
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
            await Receiver.LoopComplete();
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
        private void DoEnd()
        {
            Receiver.Discover();
            eventSlim.Dispose();
        }
        #endregion

        #region IFlowMiddleware


        Task ILifeFlow.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger($"ZeroService({ServiceName})");
            eventSlim = new ManualResetEventSlim(true);
            if (ServiceName == null)
            {
                throw new SystemException("名称为空的服务是非法的");
            }
            RealState = StationState.Initialized;
            ConfigState = StationStateType.Initialized;
            Receiver.Service = this;
            Receiver.Logger = logger;
            Receiver.Initialize();
            ResetStateMachine();

            logger.Information("Initialized");
            return Task.CompletedTask;
        }

        private bool _isDisposed;
        /// <inheritdoc/>
        Task ILifeFlow.Destory()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }
            logger.Information("Destory");
            Receiver.Destory();
            return Task.CompletedTask;
        }

        Task ILifeFlow.Open()
        {
            logger.Information("Open");
            return StateMachine.Start();
        }

        Task ILifeFlow.Closing()
        {
            logger.Information("Closing");
            return StateMachine.Closing();
        }

        Task ILifeFlow.Close()
        {
            logger.Information("Close");
            return StateMachine.Close();
        }
        #endregion

        #region 状态机接口

        Task<bool> IStateMachineControl.DoStart()
        {
            if (RealState == StationState.BeginRun || RealState == StationState.Run)
            {
                return Task.FromResult(true);//已启动,不应该再次
            }

            using (ManualResetEventSlimScope.Scope(eventSlim))
            {
                if (RealState == StationState.BeginRun || RealState == StationState.Run)
                {
                    return Task.FromResult(true);//已启动,不应该再次
                }

                if (DoStart())
                {
                    return Task.FromResult(true);
                }

                RealState = StationState.Failed;

                ResetStateMachine();
                ZeroFlowControl.OnObjectFailed(this);
                return Task.FromResult(false);
            }
        }

        async Task<bool> IStateMachineControl.DoClosing()
        {
            logger.Information("Closing  {0}", ServiceName);
            if (RealState >= StationState.Closing || CancelToken == null || CancelToken.IsCancellationRequested)
            {
                logger.Warning("[Closing] service is closed");
                return false;
            }
            RealState = StationState.Closing;
            CancelToken.Cancel();
            await Receiver.Closing();
            logger.Information("[Closing] cancel,waiting loop end... ");
            return true;
        }

        async Task<bool> IStateMachineControl.DoClose()
        {
            logger.Information("[Close] {0}", ServiceName);
            if (RealState >= StationState.Closed)
            {
                logger.Warning("[Close] service is closed");
                return false;
            }
            RealState = StationState.Closed;

            await Receiver.Close();
            logger.Information("[Closed]");
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

        ISerializeProxy serialize;

        /// <summary>
        /// 序列化对象
        /// </summary>
        public ISerializeProxy Serialize
        {
            get => serialize ??= DependencyHelper.GetService<ISerializeProxy>();
            set => serialize = value;
        }
        /// <summary>
        /// 通配事件
        /// </summary>
        IApiAction WildcardAction;

        /// <summary>
        /// 注册的方法
        /// </summary>
        readonly Dictionary<string, IApiAction> ApiActions = new Dictionary<string, IApiAction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///  取得API信息
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public IApiAction GetApiAction(string api)
        {
            return api != null && ApiActions.TryGetValue(api, out var info) ? info : WildcardAction;
        }

        /// <summary>
        ///     注册通配方法
        /// </summary>
        /// <param name="info">反射信息</param>
        void IService.RegistWildcardAction(ApiActionInfo info)
        {
            WildcardAction = new ApiAction
            {
                Info = info,
                Function = info.Action,
                ResultType = info.ResultType,
                IsAsync = info.IsAsync,
                ResultSerializeType = info.ResultSerializeType,
                ArgumentSerializeType = info.ArgumentSerializeType
            };

            WildcardAction.Initialize();
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="route">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="info">反射信息</param>
        bool IService.RegistAction(string route, ApiActionInfo info)
        {
            var apis = route.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var api in apis)
            {
                var action = new ApiAction
                {
                    Info = info,
                    IsAsync = info.IsAsync,
                    RouteName = api,
                    Function = info.Action,
                    ResultType = info.ResultType,
                    ResultSerializeType = info.ResultSerializeType,
                    ArgumentSerializeType = info.ArgumentSerializeType
                };
                if (!ApiActions.TryAdd(api, action))
                    return false;
                if (info.HaseArgument)
                {
                    var arg = info.Arguments.Values.First();
                    if (!arg.IsBaseType)
                    {
                        action.ArgumentName = arg.Name;
                        action.ArgumentType = arg.ParameterInfo.ParameterType;
                    }
                }
                action.Initialize();
            }
            return true;
        }

        #endregion
    }
}