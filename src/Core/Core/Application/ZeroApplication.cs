using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.ZeroApis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ZeroTeam.MessageMVC.Messages;
using System.Runtime.InteropServices;
using Agebull.Common.Logging;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public partial class ZeroApplication
    {
        #region State

        private static IAppMiddleware[] Middlewares;

        /// <summary>
        ///     当前应用名称
        /// </summary>
        public static string AppName => Config.AppName;

        /// <summary>
        ///     站点配置
        /// </summary>
        public static ZeroAppConfigRuntime Config { get; set; }

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
            set
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
        public static bool IsAlive => ApplicationState < StationState.Closing;

        /// <summary>
        ///     已销毁
        /// </summary>
        public static bool IsDestroy => ApplicationState == StationState.Destroy;

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
            LogRecorder.Initialize();
            Config = new ZeroAppConfigRuntime
            {
                BinPath = Environment.CurrentDirectory,
                RootPath = Environment.CurrentDirectory,
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            };
            Middlewares = IocHelper.ServiceProvider.GetServices<IAppMiddleware>().OrderBy(p => p.Level).ToArray();
            using (IocScope.CreateScope("CheckOption"))
            {
                foreach (var mid in Middlewares)
                    mid.CheckOption(Config);
            }
            IocHelper.Update();
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
            waitTask = new TaskCompletionSource<bool>();
            waitTask.Task.Wait();
        }

        static TaskCompletionSource<bool> waitTask;
        /// <summary>
        ///     执行并等待
        /// </summary>
        public static async Task RunAwaiteAsync()
        {
            await Start();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            waitTask = new TaskCompletionSource<bool>();
            await waitTask.Task;
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
            if (ApplicationState >= StationState.Closing)
            {
                return;
            }
            ZeroTrace.SystemLog("Begin shutdown...");
            ApplicationState = StationState.Closing;
            OnZeroClose();
            WaitAllObjectSafeClose();
            ApplicationState = StationState.Closed;
            OnZeroEnd();
            ApplicationState = StationState.Destroy;
            ZeroTrace.SystemLog("Application shutdown ,see you late.");
            waitTask.TrySetResult(true);
        }

        #endregion

        #endregion

    }
}