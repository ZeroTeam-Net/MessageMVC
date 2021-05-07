using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     主流程控制器
    /// </summary>
    public partial class ZeroFlowControl
    {
        #region Helper

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
                    Logger.Exception(ex);
                }
            }
            tasks.Clear();
        }
        #endregion

        #region Flow

        private static IFlowMiddleware[] Middlewares;
        #region AddIn

       internal static AddInImporter addInImporter;
        /// <summary>
        ///     插件载入,作为第零步
        /// </summary>
        public static void LoadAddIn()
        {
            addInImporter = new AddInImporter();
            addInImporter.LoadAddIn(Logger);
            addInImporter.AutoRegist(Logger);
            DependencyHelper.Flush();
        }
        #endregion

        #region Check

        static readonly ConfigChecker configChecker = new();

        internal static ILogger Logger;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"unhandledException.{DateTime.Now.Ticks}.log"), e.ExceptionObject.ToString());
        }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static void LoadConfig()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.ResetColor();
            Console.WriteLine("【基础配置】开始");
            configChecker.CheckBaseConfig();
            Console.WriteLine("【基础配置】完成");
            Console.ResetColor();
            DependencyHelper.Flush();
            Logger = DependencyHelper.LoggerFactory.CreateLogger("ZeroFlowControl");
        }
        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static async Task CheckConfig()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.Check)
                return;
            Logger.Information("【配置校验】开始");
            ZeroAppOption.Instance.SetApplicationState(StationState.Check);
            var checkers = DependencyHelper.RootProvider.GetServices<IAppChecker>();
            foreach (var checker in checkers)
            {
                try
                {
                    await checker.Check(ZeroAppOption.Instance);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "ZeroFlowControl.Check:{0}", checker.GetTypeName());
                    throw;
                }
            }
            foreach (var checker in DependencyHelper.RootProvider.GetServices<IFlowMiddleware>().OfType<IAppChecker>())
            {
                try
                {
                    await checker.Check(ZeroAppOption.Instance);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "ZeroFlowControl.Check:{0}", checker.GetTypeName());
                    throw;
                }
            }
            configChecker.CheckLast(ZeroAppOption.Instance, Logger);
            var options = DependencyHelper.RootProvider.GetServices<IZeroOption>();
            foreach(var option in options)
            {
                option.Load(true);
                Logger.Trace(() => $"〖{option.OptionName}〗{(option.IsDynamic ? "动态更新":"静态配置")}，{option.SupperUrl}\n{option.ToJson(true)}");
            }

            addInImporter.LateConfigRegist(Logger);
            ConfigurationHelper.IsLocked = true;

            DependencyHelper.Flush();
            Middlewares = DependencyHelper.RootProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            Logger.Information("【配置校验】完成");
        }

        #endregion

        #region Discove

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly) => ApiDiscover.FindApies(assembly);

        /// <summary>
        ///     发现
        /// </summary>
        public static void DiscoveAll() => ApiDiscover.FindAppDomain();

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(params Assembly[] assemblies) => ApiDiscover.FindApies(assemblies);

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
            Logger.Information("【发现】开始");
            await DiscoverAll();
            Logger.Information("【发现】完成");

            Logger.Information("【初始化】开始");
            await InitializeAll();
            Logger.Information("【初始化】完成");
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

        #region Stoping


        internal static async void OnStopping(CancellationTokenSource tokenSource)
        {
            ZeroAppOption.Instance.SetApplicationState(StationState.Pause);

            List<Task> tasks = new();
            Logger.Information("【准备关闭】开始");

            tokenSource.Cancel();
            foreach (var item in ZeroAppOption.StopActions)
            {
                Logger.Information($"{item.Name}开始");
                try
                {
                    await item.Value();
                    Logger.Information($"{item.Name}完成");
                }
                catch (Exception ex)
                {
                    Logger.Information($"{item.Name}异常\n{ex}");
                }
            }
            foreach (var service in FlowServices)
            {
                try
                {
                    Logger.Information("[准备关闭] {0}", service.ServiceName);
                    tasks.Add(service.Closing());
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[准备关闭] {0}", service.ServiceName);
                }
            }
            await WaiteAll(tasks);
            foreach (var mid in Middlewares.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    Logger.Information("[准备关闭] {0}", mid.Name);
                    tasks.Add(mid.Closing());
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[准备关闭] {0}", mid.Name);
                }
            }
            await WaiteAll(tasks);

            Logger.Information("【准备关闭】结束");
        }

        #endregion

        #region Shutdown

        /// <summary>
        ///     关闭
        /// </summary>
        public static async Task Shutdown()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.Closing)
            {
                return;
            }
            DateTime start = DateTime.Now;
            double sec;
            int cnt = 0; ;
            Logger.Information($"【等待结束】剩余请求数：({ZeroAppOption.Instance.RequestSum})...");
            while (ZeroAppOption.Instance.RequestSum > 0)
            {
                cnt++;
                sec = (DateTime.Now - start).TotalSeconds;
                if (sec > ZeroAppOption.Instance.MaxCloseSecond)
                {
                    Logger.Error($"{cnt}. 已等待{sec:F2}秒,虽然剩余请求数还有{ZeroAppOption.Instance.RequestSum}),但已超过最大等待时间({ZeroAppOption.Instance.MaxCloseSecond}秒),系统将强行关闭。");
                    break;
                }
                Logger.Information($"{cnt}. 已等待{sec:F2}秒,剩余请求数：{ZeroAppOption.Instance.RequestSum}...");
                await Task.Delay(50);
            }
            sec = (int)(DateTime.Now - start).TotalSeconds;
            if (sec > 0)
            {
                Logger.Information($"{cnt}. 等待{sec:F2}秒后请求全部结束.");
            }
            Logger.Information("【正在退出...】");

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

            ScopeRuner.DisposeLocal();
            DependencyHelper.LoggerFactory.Dispose();


            Logger.Information("【已退出，下次见！】");
            if (ZeroAppOption.Instance.IsDevelopment)
                Process.GetCurrentProcess().Kill();
        }


        /// <summary>
        ///     关闭
        /// </summary>
        static async Task CloseAll()
        {
            List<Task> tasks = new();
            Logger.Information("【关闭】开始");

            foreach (var service in FlowServices)
            {
                try
                {
                    Logger.Information("[关闭服务] {0}", service.ServiceName);
                    tasks.Add(service.Close());
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[关闭服务] {0}", service.ServiceName);
                }
            }
            await WaiteAll(tasks);
            foreach (var mid in Middlewares.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    Logger.Information("[关闭流程] {0}", mid.Name);
                    tasks.Add(mid.Close());
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[关闭流程] {0}", mid.Name);
                }
            }
            await WaiteAll(tasks);
            Logger.Information("【关闭】结束");
        }


        /// <summary>
        ///     注销
        /// </summary>
        static async Task DestoryAll()
        {
            List<Task> tasks = new();
            Logger.Information("【注销】开始");
            foreach (var action in ZeroAppOption.DestoryAction)
            {
                tasks.Add(action());
            }
            await WaiteAll(tasks);
            foreach (var service in FlowServices)
            {
                try
                {
                    Logger.Information("[注销服务] {0}", service.ServiceName);
                    tasks.Add(service.Destroy());
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[注销服务]  {0}", service.ServiceName);
                }
            }
            await WaiteAll(tasks);

            foreach (var mid in Middlewares.OrderByDescending(p => p.Level).ToArray())
            {
                try
                {
                    Logger.Information("[注销流程]  {0}", mid.Name);
                    tasks.Add(mid.Destroy());
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[注销流程]  {0}", mid.Name);
                }
            }
            await WaiteAll(tasks);
            Logger.Information("【注销】完成");
        }

        #endregion

        #endregion

        #region 服务生命周期管理

        /// <summary>
        /// 需要运行流程的服务
        /// </summary>
        public static IService[] FlowServices;

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive(IService obj)
        {
            bool can;
            Logger.Information("[OnObjectActive] {0}", obj.ServiceName);
            lock (ActiveObjects)
            {
                ActiveObjects.Add(obj);
            }
            can = ActiveObjects.Count + FailedObjects.Count == FlowServices.Length;
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
            Logger.Information("[OnObjectFailed] {0}", obj.ServiceName);
            bool can;
            lock (FailedObjects)
            {
                FailedObjects.Add(obj);
            }
            can = ActiveObjects.Count + FailedObjects.Count == FlowServices.Length;
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
            Logger.Information("[服务关闭] {0}", obj.ServiceName);
            bool can;
            lock (ActiveObjects)
            {
                ActiveObjects.Remove(obj);
            }
            can = ActiveObjects.Count == 0;
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
                if (FlowServices.Length == 0 || ActiveObjects.Count == 0)
                {
                    return;
                }
            }
            await ActiveSemaphore.WaitAsync();
        }

        #endregion

        #region Flow

        /// <summary>
        ///     发现
        /// </summary>
        static async Task DiscoverAll()
        {
            Logger.Information("【发现】开始");
            var discovers = DependencyHelper.RootProvider.GetServices<IZeroDiscover>();
            foreach (var discover in discovers)
            {
                try
                {
                    await discover.Discovery();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[发现] {0}");
                }
            }
            Middlewares = DependencyHelper.RootProvider.GetServices<IFlowMiddleware>().OrderBy(p => p.Level).ToArray();
            foreach (var mid in Middlewares)
            {
                try
                {
                    Logger.Information("[发现] {0}", mid.Name);
                    await mid.Discovery();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[发现] {0}", mid.Name);
                }
            }
            var servcies = DependencyHelper.RootProvider.GetServices<IService>();
            if (servcies != null)
            {
                foreach (var service in servcies)
                {
                    Services.TryAdd(service.ServiceName, service);
                    Logger.Information($"【服务自注册】{service.ServiceName}");
                }
            }
            DependencyHelper.Flush();
            Logger.Information("【发现】完成");
        }

        /// <summary>
        ///     初始化
        /// </summary>
        static async Task InitializeAll()
        {
            Logger.Information("【初始化】开始");
            foreach (var mid in Middlewares)
            {
                try
                {
                    Logger.Information("[初始化流程] {0}", mid.Name);
                    await mid.Initialize();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[初始化流程] {0}", mid.Name);
                }
            }

            foreach (var service in Services.Values.ToArray())
            {
                try
                {
                    Logger.Information("[初始化服务] {0}({1})", service.ServiceName,service.Receiver.GetTypeName());
                    await service.Initialize();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[初始化服务] {0}({1})", service.ServiceName, service.Receiver.GetTypeName());
                }
            }
            Logger.Information("【初始化】完成");

        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        static async Task<bool> OpenAll()
        {
            if (ZeroAppOption.Instance.ApplicationState >= StationState.BeginRun)
                return false;
            ZeroAppOption.Instance.SetApplicationState(StationState.BeginRun);
            Logger.Information("【启动】开始");
            foreach (var mid in Middlewares)
            {
                try
                {
                    Logger.Information("[启动流程] {0}", mid.Name);
                    _ = mid.Open();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[启动流程] {0}", mid.Name);
                }
            }
            FlowServices = Services.Values.Where(service => !(service is EmptyService) && (service.CanRun == null || service.CanRun())).OrderBy(p => p.Level).ToArray();
            foreach (var service in FlowServices)
            {
                try
                {
                    Logger.Information("[启动服务] {0}", service.ServiceName);
                    _ = service.Open();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[启动服务] {0}", service.ServiceName);
                }
            }
            //等待所有对象信号(Active or Failed)
            if (FlowServices.Length > 0)
                await ActiveSemaphore.WaitAsync();
            ZeroAppOption.Instance.SetApplicationState(StationState.Run);
            Logger.Information("【启动】完成");
            return true;
        }

        internal static async void OnStarted(CancellationTokenSource tokenSource)
        {
            Logger.Information("执行启动后任务");
            foreach (var item in ZeroAppOption.StartActions)
            {
                Logger.Information($"{item.Name}开始");
                try
                {
                    await item.Value(tokenSource.Token);
                    Logger.Information($"{item.Name}完成");
                }
                catch (Exception ex)
                {
                    Logger.Information($"{item.Name}异常\n{ex}");
                }
            }
            Logger.Information("启动后任务全部完成");
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
                Logger.Information("[StartFailed] all service is runing,no action");
                return;
            }

            Logger.Information("[StartFailed>>");
            FailedObjects.Clear();

            foreach (var service in faileds)
            {
                try
                {
                    Logger.Information("[StartFailed] {0}", service.ServiceName);
                    service.Open();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[StartFailed] {0}", service.ServiceName);
                }
            }

            //等待所有对象信号(全开或全关)
            ActiveSemaphore.Wait();
            Interlocked.Decrement(ref inFailed);
            Logger.Information("<<StartFailed]");
        }
        #endregion

        #region 服务注册管理

        /// <summary>
        /// 已注册的对象
        /// </summary>
        public static readonly ConcurrentDictionary<string, IService> Services = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        internal static readonly List<IService> ActiveObjects = new();

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        private static readonly List<IService> FailedObjects = new();

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim ActiveSemaphore = new(0, short.MaxValue);

        /// <summary>
        ///     取服务，内部使用
        /// </summary>
        public static IService GetService(string name)
        {
            return name != null && Services.TryGetValue(name, out var service) ? service : null;
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
                Logger?.Error("服务注册失败({0}),因为同名服务已存在", service.ServiceName);
                return false;
            }
            Logger?.Information("[注册服务:{0}] {1}(Receiver:{2})", service.ServiceName, service.GetTypeName(), service.Receiver.GetTypeName());

            if (ZeroAppOption.Instance.ApplicationState >= StationState.Initialized)
            {
                try
                {
                    Logger.Information("[初始化服务] {0}({1})", service.ServiceName, service.Receiver.GetTypeName());
                    service.Initialize();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[初始化服务] {0}({1})", service.ServiceName, service.Receiver.GetTypeName());
                }
            }
            if (ZeroAppOption.Instance.ApplicationState != StationState.Run)
            {
                return true;
            }
            try
            {
                Logger.Information("[启动服务] {0}", service.ServiceName);
                service.Open();
            }
            catch (Exception e)
            {
                Logger.Exception(e, "[启动服务] {0}", service.ServiceName);
            }
            return true;
        }

        /// <summary>
        ///     注册服务
        /// </summary>
        internal static bool RegistService(ref IService service)
        {
            if (ZeroAppOption.Instance.ServiceReplaceMap.TryGetValue(service.ServiceName, out var map))
                service.ServiceName = map;
            if (Services.TryGetValue(service.ServiceName, out var old))
            {
                service = old;
            }
            else if (!Services.TryAdd(service.ServiceName, service))
            {
                Logger.Error("服务注册失败({0}),因为同名服务已存在", service.ServiceName);
                return false;
            }
            Logger?.Information("[注册服务:{0}] {1}(Receiver:{2})", service.ServiceName, service.GetTypeName(), service.Receiver.GetTypeName());

            if (ZeroAppOption.Instance.ApplicationState >= StationState.Initialized)
            {
                try
                {
                    Logger.Information("[初始化服务] {0}({1})", service.ServiceName, service.Receiver.GetTypeName());
                    service.Initialize();
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "[初始化服务] {0}({1})", service.ServiceName, service.Receiver.GetTypeName());
                }
            }
            if (ZeroAppOption.Instance.ApplicationState != StationState.Run)
            {
                return true;
            }
            try
            {
                Logger.Information("[启动服务] {0}", service.ServiceName);
                service.Open();
            }
            catch (Exception e)
            {
                Logger.Exception(e, "[启动服务] {0}", service.ServiceName);
            }
            return true;
        }

        #endregion
    }
}