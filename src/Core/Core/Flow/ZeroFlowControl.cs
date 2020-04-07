using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

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
        private static ILogger logger;

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static void CheckOption()
        {
            if (LogRecorder.NoRegist)
                LogRecorder.DoInitialize();
            logger = IocHelper.LoggerFactory.CreateLogger(nameof(ZeroFlowControl));

            IocHelper.Update();
            Middlewares = IocHelper.RootProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            foreach (var mid in Middlewares)
            {
                try
                {
                    mid.CheckOption(ZeroAppOption.Instance);
                }
                catch (Exception ex)
                {
                    logger.Exception(ex, "ZeroFlowControl.CheckOption:{0}", mid.GetTypeName());
                    throw;
                }
            }
            IocHelper.Update();

            //显示
            Console.WriteLine($@"Wecome ZeroTeam MessageMVC
      AppName : {ZeroAppOption.Instance.AppName}
      Version : {ZeroAppOption.Instance.AppVersion}
     RunModel : {ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"]}
  ServiceName : {ZeroAppOption.Instance.ServiceName}
           OS : {(ZeroAppOption.Instance.IsLinux ? "Linux" : "Windows")}
         Host : {ZeroAppOption.Instance.LocalIpAddress}
        AddIn : {(ZeroAppOption.Instance.EnableAddIn ? "Enable" : "Disable")}({ZeroAppOption.Instance.AddInPath})
    TraceName : {ZeroAppOption.Instance.TraceName}
   ThreadPool : {ZeroAppOption.Instance.MaxWorkThreads:N0}worker|{ ZeroAppOption.Instance.MaxIOThreads:N0}threads
     RootPath : {ZeroAppOption.Instance.RootPath}
   DataFolder : {ZeroAppOption.Instance.DataFolder}
 ConfigFolder : {ZeroAppOption.Instance.ConfigFolder}
");
        }

        #endregion

        #region Discove


        private static readonly List<Assembly> knowAssemblies = new List<Assembly>();

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly)
        {
            if (knowAssemblies.Contains(assembly))
            {
                return;
            }

            knowAssemblies.Add(assembly);
            var discover = new ApiDiscover
            {
                Assembly = assembly
            };
            discover.FindApies();
        }


        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove() => Discove(AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(IEnumerable<Assembly> assemblies)
        {
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            foreach (var asm in assemblies)
            {
                //Console.WriteLine(asm.FullName);
                if (knowAssemblies.Contains(asm) ||
                    asm.FullName == null ||
                    asm.FullName.Contains("netstandard") ||
                    asm.FullName.Contains("System.") ||
                    asm.FullName.Contains("Microsoft.") ||
                    asm.FullName.Contains("Newtonsoft.") ||
                    asm.FullName.Contains("Agebull.Common.") ||
                    asm.FullName.Contains("ZeroTeam.MessageMVC.Abstractions") ||
                    asm.FullName.Contains("ZeroTeam.MessageMVC.Core"))
                {
                    knowAssemblies.Add(asm);
                    continue;
                }
                Discove(asm);
            }
        }

        //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    Discove(args.RequestingAssembly);
        //    return null;
        //}

        #endregion

        #region Initialize

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            IocHelper.Update();
            Middlewares = IocHelper.RootProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            var servcies = IocHelper.RootProvider.GetServices<IService>();
            if (servcies != null)
            {
                foreach (var service in servcies)
                {
                    Services.TryAdd(service.ServiceName, service);
                }
            }
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
            var task = OnZeroStart();
            task.Wait();
            return task.Result;
        }

        /// <summary>
        ///     运行
        /// </summary>
        public static Task<bool> RunAsync()
        {
            return OnZeroStart();
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            OnZeroStart().Wait();
            waitTask = new TaskCompletionSource<bool>();
            waitTask.Task.Wait();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            Console.WriteLine("MicroZero services is runing. Press Ctrl+C to shutdown.");
        }

        private static TaskCompletionSource<bool> waitTask;
        /// <summary>
        ///     执行并等待
        /// </summary>
        public static async Task RunAwaiteAsync()
        {
            await OnZeroStart();
            waitTask = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            Console.WriteLine("MicroZero services is runing. Press Ctrl+C to shutdown.");
            await waitTask.Task;
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
            logger.Information("Begin shutdown...");
            ApplicationState = StationState.Closing;
            OnZeroClose();
            WaitAllObjectSafeClose();
            ApplicationState = StationState.Closed;
            OnZeroEnd();
            ApplicationState = StationState.Destroy;
            logger.Information("Application shutdown ,see you late.");
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
        ///     取服务，内部使用
        /// </summary>
        public static IService GetService(string name)
        {
            return name != null && Services.TryGetValue(name, out var service) ? service : null;
        }
        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive(IService obj)
        {
            bool can;
            logger.Information("[OnObjectActive] {0}", obj.ServiceName);
            lock (ActiveObjects)
            {
                ActiveObjects.Add(obj);
                can = ActiveObjects.Count + FailedObjects.Count == Services.Count;
            }
            if (can)
            {
                ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectFailed(IService obj)
        {
            logger.Information("[OnObjectFailed] {0}", obj.ServiceName);
            bool can;
            lock (ActiveObjects)
            {
                FailedObjects.Add(obj);
                can = ActiveObjects.Count + FailedObjects.Count == Services.Count;
            }
            if (can)
            {
                ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectClose(IService obj)
        {
            logger.Information("[OnObjectClose] {0}", obj.ServiceName);
            bool can;
            lock (ActiveObjects)
            {
                ActiveObjects.Remove(obj);
                can = ActiveObjects.Count == 0;
            }
            if (can)
            {
                ActiveSemaphore.Release(); //发出完成信号
            }

        }

        /// <summary>
        ///     等待所有对象信号(全开或全关)
        /// </summary>
        private static void WaitAllObjectSafeClose()
        {
            lock (ActiveObjects)
            {
                if (Services.Count == 0 || ActiveObjects.Count == 0)
                {
                    return;
                }
            }

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
            {
                return false;
            }
            logger?.Information("[RegistService] {0}", service.ServiceName);

            if (ApplicationState >= StationState.Initialized)
            {
                try
                {
                    logger.Information("[Initialize]", service.ServiceName);
                    service.Initialize();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[Initialize]", service.ServiceName);
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
            {
                return true;
            }

            try
            {
                logger.Information("[Start]", service.ServiceName);
                service.Start();
            }
            catch (Exception e)
            {
                logger.Exception(e, "[Start]", service.ServiceName);
            }
            return true;
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            logger.Information("[OnZeroInitialize>>");
            foreach (var mid in Middlewares)
            {
                try
                {
                    logger.Information("[Initialize]", mid.Name);
                    mid.Initialize();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[Initialize]", mid.Name);
                }
            }

            foreach (var service in Services.Values.ToArray())
            {
                try
                {
                    logger.Information("[Initialize]", service.ServiceName);
                    service.Initialize();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[Initialize]", service.ServiceName);
                }
            }
            logger.Information("<<OnZeroInitialize]");
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static async Task<bool> OnZeroStart()
        {
            ApplicationState = StationState.BeginRun;
            logger.Information("[OnZeroStart>>");
            foreach (var mid in Middlewares)
            {
                try
                {
                    logger.Information("[Start]", mid.Name);
                    mid.Start();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[Start]", mid.Name);
                }
            }

            foreach (var service in Services.Values.ToArray())
            {
                try
                {
                    logger.Information("[Start]", service.ServiceName);
                    _ = Task.Run(service.Start);
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[Start]", service.ServiceName);
                }
            }

            //等待所有对象信号(Active or Failed)
            await ActiveSemaphore.WaitAsync();

            ApplicationState = StationState.Run;
            logger.Information("<<OnZeroStart]");
            return true;
        }

        private static int inFailed = 0;

        /// <summary>
        ///     重新启动未正常启动的项目
        /// </summary>
        public static void StartFailed()
        {
            if (Interlocked.Increment(ref inFailed) != 1)
            {
                return;
            }

            var faileds = FailedObjects.ToArray();
            if (faileds.Length == 0)
            {
                logger.Information("[StartFailed] all service is runing,no action");
                return;
            }

            logger.Information("[StartFailed>>");
            FailedObjects.Clear();

            foreach (var service in faileds)
            {
                try
                {
                    logger.Information("[StartFailed]", service.ServiceName);
                    service.Start();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[StartFailed]", service.ServiceName);
                }
            }

            //等待所有对象信号(全开或全关)
            ActiveSemaphore.Wait();
            Interlocked.Decrement(ref inFailed);
            logger.Information("<<StartFailed]");
        }
        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroClose()
        {
            logger.Information("[OnZeroClose>>");
            foreach (var mid in Middlewares)
            {
                try
                {
                    logger.Information("[OnZeroClose]", mid.Name);
                    mid.Close();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[OnZeroClose]", mid.Name);
                }
            }

            foreach (var service in Services.Values)
            {
                try
                {
                    logger.Information("[OnZeroClose]", service.ServiceName);
                    service.Close();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[OnZeroClose]", service.ServiceName);
                }
            }
            logger.Information("<<OnZeroClose]");

            GC.Collect();
        }

        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroEnd()
        {
            logger.Information("[OnZeroEnd>>");
            foreach (var mid in Middlewares)
            {
                try
                {
                    logger.Information("[OnZeroEnd]", mid.Name);
                    mid.End();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[OnZeroEnd]", mid.Name);
                }
            }

            foreach (var service in Services.Values)
            {
                try
                {
                    logger.Information("[OnZeroEnd]", service.ServiceName);
                    service.End();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "[OnZeroEnd]", service.ServiceName);
                }
            }
            logger.Information("<<OnZeroEnd]");
        }

        #endregion
    }
}