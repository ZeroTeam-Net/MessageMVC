using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        #region Flow

        private static IFlowMiddleware[] Middlewares;

        #region Check

        internal static ILogger _logger;

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static async Task Check()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.Check)
                return;
            ZeroAppOption.Instance.SetApplicationState(StationState.Check);
            //LoggerExtend.DoInitialize();

            DependencyHelper.Flush();
            Middlewares = DependencyHelper.RootProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            foreach (var mid in Middlewares)
            {
                try
                {
                    await mid.Check(ZeroAppOption.Instance);
                }
                catch (Exception ex)
                {
                    _logger.Exception(ex, "ZeroFlowControl.Check:{0}", mid.GetTypeName());
                    throw;
                }
            }
            DependencyHelper.Flush();

            //显示
            Console.WriteLine($@"Wecome ZeroTeam MessageMVC
       AppName : {ZeroAppOption.Instance.AppName}
       Version : {ZeroAppOption.Instance.AppVersion}
      RunModel : {(ZeroAppOption.Instance.IsDevelopment ? "Development" : "Production")}
   ServiceName : {ZeroAppOption.Instance.ServiceName}
ApiServiceName : {ZeroAppOption.Instance.ApiServiceName}
            OS : {(ZeroAppOption.Instance.IsLinux ? "Linux" : "Windows")}
          Host : {ZeroAppOption.Instance.LocalIpAddress}
     AddInPath : {ZeroAppOption.Instance.AddInPath}
     TraceName : {ZeroAppOption.Instance.TraceName}
    ThreadPool : {ZeroAppOption.Instance.MaxWorkThreads:N0}worker|{ ZeroAppOption.Instance.MaxIOThreads:N0}threads
      RootPath : {ZeroAppOption.Instance.RootPath}
     DataFolder : {ZeroAppOption.Instance.DataFolder}
 ConfigFolder : {ZeroAppOption.Instance.ConfigFolder}
");
        }//(ZeroAppOption.Instance.EnableAddIn ? "Enable" : "Disable")}(

        #endregion

        #region Discove

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly) => ApiDiscover.FindApies(assembly);

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove() => ApiDiscover.FindAppDomain();

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(params Assembly[] assemblies) => ApiDiscover.FindApies( assemblies);

        #endregion

        #region Initialize

        /// <summary>
        ///     初始化
        /// </summary>
        public static async Task Initialize()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.Initialized)
                return;
            ZeroAppOption.Instance.SetApplicationState(StationState.Initialized);
            DependencyHelper.Flush();
            await DiscoverAll();
            DependencyHelper.Flush();
            Middlewares = DependencyHelper.RootProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            var servcies = DependencyHelper.RootProvider.GetServices<IService>();
            if (servcies != null)
            {
                foreach (var service in servcies)
                {
                    Services.TryAdd(service.ServiceName, service);
                }
            }
            await InitializeAll();
            DependencyHelper.Flush();
        }

        #endregion

        #region Run

        /// <summary>
        ///     运行
        /// </summary>
        public static Task<bool> RunAsync()
        {
            return OpenAll();
        }

        #endregion


        #endregion

        #region IService

        /// <summary>
        /// 已注册的对象
        /// </summary>
        public static readonly ConcurrentDictionary<string, IService> Services = new ConcurrentDictionary<string, IService>(StringComparer.OrdinalIgnoreCase);

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
            _logger.Information("[OnObjectActive] {0}", obj.ServiceName);
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
            _logger.Information("[OnObjectFailed] {0}", obj.ServiceName);
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
            _logger.Information("[服务关闭] {0}", obj.ServiceName);
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
        private static async Task WaitAllObjectSafeClose()
        {
            lock (ActiveObjects)
            {
                if (Services.Count == 0 || ActiveObjects.Count == 0)
                {
                    return;
                }
            }
            await ActiveSemaphore.WaitAsync();
        }

        /// <summary>
        ///     取已注册对象
        /// </summary>
        public static IService TryGetZeroObject(string name)
        {
            return Services.TryGetValue(name, out var zeroObject) ? zeroObject : null;
        }

        /// <summary>
        ///     注册服务
        /// </summary>
        public static bool RegistService(IService service)
        {
            if (!Services.TryAdd(service.ServiceName, service))
            {
                _logger?.Error("服务注册失败({0}),因为同名服务已存在", service.ServiceName);
                return false;
            }
            _logger?.Information("[注册服务] {0}{1}", service.ServiceName, service.Receiver.GetTypeName());

            if (ZeroAppOption.Instance.ApplicationState >= StationState.Initialized)
            {
                try
                {
                    _logger.Information("[初始化服务] {0}", service.ServiceName);
                    service.Initialize();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[初始化服务] {0}", service.ServiceName);
                }
            }
            if (ZeroAppOption.Instance.ApplicationState != StationState.Run)
            {
                return true;
            }
            try
            {
                _logger.Information("[启动服务] {0}", service.ServiceName);
                service.Open();
            }
            catch (Exception e)
            {
                _logger.Exception(e, "[启动服务] {0}", service.ServiceName);
            }
            return true;
        }

        /// <summary>
        ///     注册服务
        /// </summary>
        public static bool RegistService(ref IService service)
        {
            _logger.Information("[注册服务] {0}", service.ServiceName);

            if (ZeroAppOption.Instance.ServiceMap.TryGetValue(service.ServiceName, out var map))
                service.ServiceName = map;
            if (Services.TryGetValue(service.ServiceName, out var old))
            {
                service = old;
            }
            else if (!Services.TryAdd(service.ServiceName, service))
            {
                _logger.Error("服务注册失败({0}),因为同名服务已存在", service.ServiceName);
                return false;
            }

            if (ZeroAppOption.Instance.ApplicationState >= StationState.Initialized)
            {
                try
                {
                    _logger.Information("[初始化服务] {0}", service.ServiceName);
                    service.Initialize();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[初始化服务] {0}", service.ServiceName);
                }
            }
            if (ZeroAppOption.Instance.ApplicationState != StationState.Run)
            {
                return true;
            }
            try
            {
                _logger.Information("[启动服务] {0}", service.ServiceName);
                service.Open();
            }
            catch (Exception e)
            {
                _logger.Exception(e, "[启动服务] {0}", service.ServiceName);
            }
            return true;
        }

        /// <summary>
        ///     发现
        /// </summary>
        static async Task DiscoverAll()
        {
            _logger.Information("【发现】开始");
            foreach (var mid in Middlewares)
            {
                try
                {
                    _logger.Information("[发现] {0}", mid.Name);
                    await mid.Discover();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[发现] {0}", mid.Name);
                }
            }
            _logger.Information("【发现】完成");
        }

        /// <summary>
        ///     初始化
        /// </summary>
        static async Task InitializeAll()
        {
            _logger.Information("【初始化】开始");
            foreach (var mid in Middlewares)
            {
                try
                {
                    _logger.Information("[初始化流程] {0}", mid.Name);
                    await mid.Initialize();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[初始化流程] {0}", mid.Name);
                }
            }

            foreach (var service in Services.Values.ToArray())
            {
                try
                {
                    _logger.Information("[初始化服务] {0}", service.ServiceName);
                    await service.Initialize();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[初始化服务] {0}", service.ServiceName);
                }
            }
            _logger.Information("【初始化】完成");

        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        static async Task<bool> OpenAll()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.BeginRun)
                return false;
            ZeroAppOption.Instance.SetApplicationState(StationState.BeginRun);
            _logger.Information("【启动】开始");
            foreach (var mid in Middlewares)
            {
                try
                {
                    _logger.Information("[启动流程] {0}", mid.Name);
                    _ = mid.Open();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[启动流程] {0}", mid.Name);
                }
            }

            foreach (var service in Services.Values.OrderBy(p => p.Level).ToArray())
            {
                try
                {
                    _logger.Information("[启动服务] {0}", service.ServiceName);
                    _ = service.Open();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[启动服务] {0}", service.ServiceName);
                }
            }
            //等待所有对象信号(Active or Failed)
            if (Services.Count > 0)
                await ActiveSemaphore.WaitAsync();
            ZeroAppOption.Instance.SetApplicationState(StationState.Run);
            _logger.Information("【启动】完成");
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
                _logger.Information("[StartFailed] all service is runing,no action");
                return;
            }

            _logger.Information("[StartFailed>>");
            FailedObjects.Clear();

            foreach (var service in faileds)
            {
                try
                {
                    _logger.Information("[StartFailed] {0}", service.ServiceName);
                    service.Open();
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[StartFailed] {0}", service.ServiceName);
                }
            }

            //等待所有对象信号(全开或全关)
            ActiveSemaphore.Wait();
            Interlocked.Decrement(ref inFailed);
            _logger.Information("<<StartFailed]");
        }
        /// <summary>
        /// 等所有Task完成
        /// </summary>
        /// <param name="tasks"></param>
        static async Task WaiteAll(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    _logger.Exception(ex);
                }
            }
            tasks.Clear();
        }
        #endregion

        #region 关闭

        /// <summary>
        ///     关闭
        /// </summary>
        public static async Task Shutdown()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.Closing)
            {
                return;
            }
            _logger.Information("【正在退出...】");

            ZeroAppOption.Instance.SetApplicationState(StationState.Closing);
            await CloseAll();
            if (ZeroAppOption.Instance.ApplicationState != StationState.Closing)
            {
                return;
            }
            await WaitAllObjectSafeClose();
            ZeroAppOption.Instance.SetApplicationState(StationState.Closed);
            await DestoryAll();
            ZeroAppOption.Instance.SetApplicationState(StationState.Destroy);

            DependencyRun.DisposeLocal();
            DependencyHelper.LoggerFactory.Dispose();
            await ZeroAppOption.Destory();

            _logger.Information("【已退出，下次见！】");
            if (ZeroAppOption.Instance.IsDevelopment)
                Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        ///     关闭
        /// </summary>
        static async Task CloseAll()
        {
            List<Task> tasks = new List<Task>();
            _logger.Information("【关闭】开始");

            foreach (var service in Services.Values.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    _logger.Information("[关闭服务] {0}", service.ServiceName);
                    tasks.Add(service.Close());
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[关闭服务] {0}", service.ServiceName);
                }
            }
            await WaiteAll(tasks);
            foreach (var mid in Middlewares.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    _logger.Information("[关闭流程] {0}", mid.Name);
                    tasks.Add(mid.Close());
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[关闭流程] {0}", mid.Name);
                }
            }
            await WaiteAll(tasks);
            _logger.Information("【关闭】结束");
        }
        /// <summary>
        ///     注销
        /// </summary>
        static async Task DestoryAll()
        {
            List<Task> tasks = new List<Task>();
            _logger.Information("【注销】开始");
            foreach (var service in Services.Values.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    _logger.Information("[注销服务] {0}", service.ServiceName);
                    tasks.Add(service.Destory());
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[注销服务]  {0}", service.ServiceName);
                }
            }
            await WaiteAll(tasks);

            foreach (var mid in Middlewares.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    _logger.Information("[注销流程]  {0}", mid.Name);
                    tasks.Add(mid.Destory());
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "[注销流程]  {0}", mid.Name);
                }
            }
            await WaiteAll(tasks);
            _logger.Information("【注销】完成");
        }

        #endregion

        #region 开始结束任务

        static readonly List<NameValue<string, Func<CancellationToken, Task>>> StartActions = new List<NameValue<string, Func<CancellationToken, Task>>>();

        static readonly List<NameValue<string, Func<Task>>> StopActions = new List<NameValue<string, Func<Task>>>();

        /// <summary>
        /// 注册后台方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegistStartAction(string name, Func<CancellationToken, Task> action)
            => StartActions.Add(new NameValue<string, Func<CancellationToken, Task>>
            {
                Name = name,
                Value = action
            });

        /// <summary>
        /// 注册关机方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegistStopAction(string name, Func<Task> action)
            => StopActions.Add(new NameValue<string, Func<Task>>
            {
                Name = name,
                Value = action
            });

        internal static async void OnStarted(CancellationTokenSource tokenSource)
        {
            _logger.LogInformation("执行启动后任务");
            foreach (var item in StartActions)
            {
                _logger.LogInformation($"{item.Name}开始");
                try
                {
                    await item.Value(tokenSource.Token);
                    _logger.LogInformation($"{item.Name}完成");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"{item.Name}异常\n{ex}");
                }
            }
            _logger.LogInformation("启动后任务全部完成");
        }

        internal static async void OnStopping(CancellationTokenSource tokenSource)
        {
            _logger.LogInformation("执行关闭前任务");
            tokenSource.Cancel();
            foreach (var item in StopActions)
            {
                _logger.LogInformation($"{item.Name}开始");
                try
                {
                    await item.Value();
                    _logger.LogInformation($"{item.Name}完成");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"{item.Name}异常\n{ex}");
                }
            }
            _logger.LogInformation("关闭前任务全部完成");
        }
        #endregion

    }
}