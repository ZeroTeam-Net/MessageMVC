using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.ZeroApis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Runtime.InteropServices;
using Agebull.Common.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     主流程控制器
    /// </summary>
    public partial class ZeroFlowControl
    {
        #region State

        private static IFlowMiddleware[] Middlewares;

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

        #region CheckOption

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
            Middlewares = IocHelper.ServiceProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            using (IocScope.CreateScope("CheckOption"))
            {
                foreach (var mid in Middlewares)
                {
                    try
                    {
                        mid.CheckOption(Config);
                    }
                    catch (Exception ex)
                    {
                        LogRecorder.Exception(ex, "ZeroFlowControl.CheckOption:{0}", mid.GetTypeName());
                        throw;
                    }
                }
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
            OnZeroInitialize();
            ApplicationState = StationState.Initialized;
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
            OnZeroStart().Wait();
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
            await OnZeroStart();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            waitTask = new TaskCompletionSource<bool>();
            await waitTask.Task;
        }

        /// <summary>
        ///     启动
        /// </summary>
        private static Task<bool> Start()
        {
            return OnZeroStart();
        }

        #endregion

        #region Shutdown

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
            waitTask?.TrySetResult(true);
        }

        #endregion

        #endregion

        #region IService

        /// <summary>
        /// 已注册的对象
        /// </summary>
        internal static readonly ConcurrentDictionary<string, IService> Services = new ConcurrentDictionary<string, IService>();

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        internal static readonly List<IService> ActiveObjects = new List<IService>();

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        private static readonly List<IService> FailedObjects = new List<IService>();

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim ActiveSemaphore = new SemaphoreSlim(0, short.MaxValue);

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive(IService obj)
        {
            bool can;
            ZeroTrace.SystemLog(obj.ServiceName, "OnObjectActive");
            lock (ActiveObjects)
            {
                ActiveObjects.Add(obj);
                can = ActiveObjects.Count + FailedObjects.Count == Services.Count;
            }
            if (can)
                ActiveSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectFailed(IService obj)
        {
            ZeroTrace.WriteError(obj.ServiceName, "OnObjectFailed");
            bool can;
            lock (ActiveObjects)
            {
                FailedObjects.Add(obj);
                can = ActiveObjects.Count + FailedObjects.Count == Services.Count;
            }
            if (can)
                ActiveSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectClose(IService obj)
        {
            ZeroTrace.SystemLog(obj.ServiceName, "OnObjectClose");
            bool can;
            lock (ActiveObjects)
            {
                ActiveObjects.Remove(obj);
                can = ActiveObjects.Count == 0;
            }
            if (can)
                ActiveSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     等待所有对象信号(全开或全关)
        /// </summary>
        private static void WaitAllObjectSafeClose()
        {
            lock (ActiveObjects)
                if (Services.Count == 0 || ActiveObjects.Count == 0)
                    return;
            ActiveSemaphore.Wait();
        }

        /// <summary>
        ///     取已注册对象
        /// </summary>
        public static IService TryGetZeroObject(string name)
        {
            return Services.TryGetValue(name, out var zeroObject) ? zeroObject : null;
        }

        /// <summary>
        ///     注册对象
        /// </summary>
        public static bool RegistService(IService service)
        {
            if (!Services.TryAdd(service.ServiceName, service))
                return false;
            ZeroTrace.SystemLog(service.ServiceName, "RegistZeroObject");

            if (ApplicationState >= StationState.Initialized)
            {
                try
                {
                    service.Initialize();
                    ZeroTrace.SystemLog(service.ServiceName, "Initialize");
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(service.ServiceName, e, "Initialize");
                }
            }

            //if (obj.GetType().IsSubclassOf(typeof(ZeroStation)))
            //{
            //    ZeroDiscover discover = new ZeroDiscover
            //    {
            //        StationName = obj.StationName
            //    };
            //    discover.FindApies(obj.GetType());
            //    ZeroDiscover.DiscoverApiDocument(obj.GetType());
            //}

            if (ApplicationState != StationState.Run)
                return true;
            try
            {
                ZeroTrace.SystemLog(service.ServiceName, "Start");
                service.Start();
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(service.ServiceName, e, "Start");
            }
            return true;
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroInitialize>>");
            foreach (var mid in Middlewares)
                mid.Initialize();
            foreach (var obj in Services.Values.ToArray())
            {
                try
                {
                    obj.Initialize();
                    ZeroTrace.SystemLog(obj.ServiceName, "Initialize");
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.ServiceName, e, "*Initialize");
                }
            }
            ZeroTrace.SystemLog("Application", "<<OnZeroInitialize]");
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static async Task<bool> OnZeroStart()
        {
            ApplicationState = StationState.BeginRun;
            ZeroTrace.SystemLog("Application", "[OnZeroStart>>");
            foreach (var mid in Middlewares)
                _ = Task.Factory.StartNew(mid.Start);

            foreach (var obj in Services.Values.ToArray())
            {
                try
                {
                    ZeroTrace.SystemLog(obj.ServiceName, $"Try start by {StationState.Text(obj.RealState)}");
                    _ = Task.Run(obj.Start);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.ServiceName, e, "*Start");
                }
            }

            //等待所有对象信号(全开或全关)
            await ActiveSemaphore.WaitAsync();

            ApplicationState = StationState.Run;
            ZeroTrace.SystemLog("Application", "<<OnZeroStart]");
            return true;
        }
        static int inFailed = 0;

        /// <summary>
        ///     重新启动未正常启动的项目
        /// </summary>
        public static void StartFailed()
        {
            if (Interlocked.Increment(ref inFailed) != 1)
                return;
            var faileds = FailedObjects.ToArray();
            if (faileds.Length == 0)
                return;
            FailedObjects.Clear();
            ZeroTrace.SystemLog("Application", "[OnFailedStart>>");

            foreach (var obj in faileds)
            {
                try
                {
                    ZeroTrace.SystemLog(obj.ServiceName, $"Try start by {StationState.Text(obj.RealState)}");
                    obj.Start();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.ServiceName, e, "*Start");
                }
            }

            //等待所有对象信号(全开或全关)
            ActiveSemaphore.Wait();
            Interlocked.Decrement(ref inFailed);
            ZeroTrace.SystemLog("Application", "<<OnFailedStart]");
        }
        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroClose()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroClose>>");
            foreach (var mid in Middlewares)
                mid.Close();
            foreach (var obj in Services.Values)
            {
                try
                {
                    ZeroTrace.SystemLog("OnZeroClose", obj.ServiceName);
                    obj.Close();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("OnZeroClose", e, obj.ServiceName);
                }
            }
            ZeroTrace.SystemLog("Application", "<<OnZeroClose]");

            GC.Collect();
        }

        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroEnd()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroEnd>>");
            foreach (var mid in Middlewares)
                mid.End();
            foreach (var obj in Services.Values)
            {
                try
                {
                    ZeroTrace.SystemLog("OnZeroEnd", obj.ServiceName);
                    obj.End();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("OnZeroEnd", e, obj.ServiceName);
                }
            }
            ZeroTrace.SystemLog("Application", "<<OnZeroEnd]");
        }

        #endregion
    }
}