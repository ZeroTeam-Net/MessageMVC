using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{

    /// <summary>
    ///     站点应用
    /// </summary>
    partial class ZeroApplication
    {

        #region IZeroObject

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
                    return ;
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
        public static bool RegistZeroObject(IService obj)
        {
            if (!Services.TryAdd(obj.ServiceName, obj))
                return false;
            ZeroTrace.SystemLog(obj.ServiceName, "RegistZeroObject");

            if (ApplicationState >= StationState.Initialized)
            {
                try
                {
                    obj.Initialize();
                    ZeroTrace.SystemLog(obj.ServiceName, "Initialize");
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.ServiceName, e, "Initialize");
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
                ZeroTrace.SystemLog(obj.ServiceName, "Start");
                obj.Start();
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(obj.ServiceName, e, "Start");
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
        internal static async Task OnZeroStart()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroStart>>");
            foreach (var mid in Middlewares)
                _ = Task.Factory.StartNew(mid.Start);
            List<Task> tasks = new List<Task>();
            foreach (var obj in Services.Values.ToArray())
            {
                try
                {
                    ZeroTrace.SystemLog(obj.ServiceName, $"Try start by {StationState.Text(obj.RealState)}");
                    tasks.Add(Task.Run(obj.Start));
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.ServiceName, e, "*Start");
                }
            }

            Task.WaitAll(tasks.ToArray());
            //等待所有对象信号(全开或全关)
            await ActiveSemaphore.WaitAsync();

            ApplicationState = StationState.Run;
            ZeroTrace.SystemLog("Application", "<<OnZeroStart]");
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
                mid.Close();
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