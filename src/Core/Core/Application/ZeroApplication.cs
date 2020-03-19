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
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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

        private static IAppMiddleware[] Middlewares;
        private static readonly CancellationTokenSource CancelToken = new CancellationTokenSource();

        /// <summary>
        ///     应用程序等待结果的信号量对象
        /// </summary>
        private static readonly SemaphoreSlim WaitToken = new SemaphoreSlim(0);

        #region Option

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static void CheckOption()
        {
            CheckConfig();
            Middlewares = IocHelper.ServiceProvider.GetServices<IAppMiddleware>().ToArray();
            foreach (var mid in Middlewares)
                mid.CheckOption(Config);
            ShowOptionInfo();
        }

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly)
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
            foreach (var mid in Middlewares)
                mid.Initialize();
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

        private static void OnConsoleOnCancelKeyPress(object s, ConsoleCancelEventArgs e)
        {
            Shutdown();
        }

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Shutdown()
        {
            ZeroTrace.SystemLog("Begin shutdown...");
            if (ApplicationState >= StationState.Closing)
            {
                return;
            }

            ApplicationState = StationState.Closing;
            WaitToken.Release();
            ApplicationState = StationState.Closed;
            OnZeroClose();

            ApplicationState = StationState.Destroy;
            OnZeroEnd();
            if (GlobalObjects.Count > 0)
                GlobalSemaphore.Wait();
            ApplicationState = StationState.Disposed;
            CancelToken.Cancel();
            ZeroTrace.SystemLog("Application shutdown ,see you late.");
        }

        #endregion

        #endregion

    }
}