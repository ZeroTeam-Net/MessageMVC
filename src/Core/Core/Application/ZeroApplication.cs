using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.ZeroApis;
using Agebull.EntityModel.Common;


using Microsoft.Extensions.Logging;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public partial class ZeroApplication
    {
        #region State

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _appState;

        /// <summary>
        ///     状态
        /// </summary>
        public static int ApplicationState
        {
            get => _appState;
            internal set
            {
                Interlocked.Exchange(ref _appState, value);
            }
        }

        /// <summary>
        /// 设置ZeroCenter与Application状态都为Failed
        /// </summary>
        public static void SetFailed()
        {
            ApplicationState = StationState.Failed;
        }

        /// <summary>
        ///     本地应用是否正在运行
        /// </summary>
        public static bool ApplicationIsRun => ApplicationState == StationState.BeginRun || ApplicationState == StationState.Run;

        /// <summary>
        ///     运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo => ApplicationIsRun;

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

        #endregion

        #region Flow


        #region Option

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static void CheckOption()
        {
            
            Console.WriteLine("Weconme MicroZero");

            ThreadPool.GetMaxThreads(out var worker, out _);
            ThreadPool.SetMaxThreads(worker, 4096);
            //ThreadPool.GetAvailableThreads(out worker, out io);

            CheckConfig();
            InitializeDependency();
            ShowOptionInfo();
        }


        /// <summary>
        ///     设置LogRecorder的依赖属性(内部使用)
        /// </summary>
        private static void InitializeDependency()
        {
            GlobalContext.ServiceName = Config.ServiceName;
            GlobalContext.ServiceRealName = $"{Config.ServiceName}:{Config.StationName}:{RandomOperate.Generate(4)}";

            //日志
            LogRecorder.LogPath = Config.LogFolder;
            LogRecorder.GetMachineNameFunc = () => GlobalContext.ServiceRealName;
            LogRecorder.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.UserId.ToString() ?? "*";
            LogRecorder.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Request?.RequestId ?? RandomOperate.Generate(10);
            LogRecorder.Initialize();
            IocScope.Logger = IocHelper.Create<ILoggerFactory>().CreateLogger("MicroZero");

            //插件
            AddInImporter.Import();
            AddInImporter.Instance.Initialize();

            //注册默认广播
           // IocHelper.AddSingleton<IZeroPublisher, ZPublisher>();

            var testContext = IocHelper.Create<GlobalContext>();
            if (testContext == null)
                IocHelper.AddScoped<GlobalContext, GlobalContext>();

        }


        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly, string stationName = null)
        {
            var discover = new ApiDiscover
            {
                Assembly = assembly
            };
            ZeroTrace.SystemLog("Discove", discover.Assembly.FullName);
            discover.FindApies();
        }

        #endregion

        #region Initialize

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            AddInImporter.Instance.AutoRegist();
            ApplicationState = StationState.Initialized;
            OnZeroInitialize();
            IocHelper.Update();
        }

        #endregion

        #region Run

        /// <summary>
        ///     运行
        /// </summary>
        public static bool Run()
        {
            var task = Start();
            task.Wait();
            return task.Result;
        }

        /// <summary>
        ///     运行
        /// </summary>
        public static async Task<bool> RunAsync()
        {
            return await Start();
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            Start().Wait();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            WaitToken.Wait(CancelToken.Token);
        }

        private static readonly CancellationTokenSource CancelToken = new CancellationTokenSource();
        /// <summary>
        ///     执行并等待
        /// </summary>
        public static async Task RunAwaiteAsync()
        {
            await Start();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            await WaitToken.WaitAsync(CancelToken.Token);
        }

        private static void OnConsoleOnCancelKeyPress(object s, ConsoleCancelEventArgs e)
        {
            Shutdown();
        }

        /// <summary>
        ///     应用程序等待结果的信号量对象
        /// </summary>
        private static readonly SemaphoreSlim WaitToken = new SemaphoreSlim(0);

        /// <summary>
        ///     启动
        /// </summary>
        private static async Task<bool> Start()
        {
            ApplicationState = StationState.Start;
            ApplicationState = StationState.BeginRun;
            await OnZeroStart();
            return true;
        }

        #endregion

        #region Destroy

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Shutdown()
        {
            ZeroTrace.SystemLog("Begin shutdown...");
            switch (ApplicationState)
            {
                case StationState.Destroy:
                    return;
            }
            ApplicationState = StationState.Destroy;
            if (GlobalObjects.Count > 0)
                GlobalSemaphore.Wait();
            OnZeroDestory();
            ApplicationState = StationState.Disposed;

            ZeroTrace.SystemLog("Application shutdown ,see you late.");

            WaitToken.Release();
            CancelToken.Cancel();
        }

        #endregion

        #endregion

    }
}