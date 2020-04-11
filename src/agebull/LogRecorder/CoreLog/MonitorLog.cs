using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Agebull.Common.Logging
{
    /// <summary>
    ///   文本记录器
    /// </summary>
    public partial class LogRecorder
    {

        /// <summary>
        /// 当前范围数据
        /// </summary>
        internal static MonitorItem MonitorItem => IocScope.Dependency.Dependency<MonitorItem>();

        /// <summary>
        /// 开始检测资源
        /// </summary>
        public static void BeginMonitor(string title)
        {
            if (!LogMonitor)
            {
                return;
            }
            var item = new MonitorItem();
            item.BeginMonitor(title);
            IocScope.Dependency.Annex(item);
        }

        /// <summary>
        /// 刷新资源检测
        /// </summary>
        public static void BeginStepMonitor(string title)
        {
            if (!LogMonitor)
            {
                return;
            }

            MonitorItem.BeginStep(title);
        }

        /// <summary>
        /// 刷新资源检测
        /// </summary>
        public static void EndStepMonitor()
        {
            if (!LogMonitor)
            {
                return;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }

            item.EndStep();
        }

        /// <summary>
        /// 显示监视跟踪
        /// </summary>
        public static bool MonitorTrace(object message)
        {
            if (!LogMonitor)
            {
                return false;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return false;
            }

            item.Write(message.ToString(), MonitorItem.ItemType.Item, false);
            return true;
        }

        /// <summary>
        /// 显示监视跟踪
        /// </summary>
        public static bool MonitorTrace(string message)
        {
            if (!LogMonitor)
            {
                return false;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return false;
            }

            item.Write(message, MonitorItem.ItemType.Item, false);
            return true;
        }

        /// <summary>
        /// 显示监视跟踪
        /// </summary>
        public static bool MonitorTrace(string message, params object[] args)
        {
            if (!LogMonitor || message == null)
            {
                return false;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return false;
            }

            item.Write(string.Format(message, args), MonitorItem.ItemType.Item, false);
            return true;
        }

        /// <summary>
        /// 显示监视跟踪
        /// </summary>
        public static bool MonitorTrace(Func<string> message)
        {
            if (!LogMonitor)
            {
                return false;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return false;
            }

            item.Write(message(), MonitorItem.ItemType.Item, false);
            return true;
        }

        /// <summary>
        /// 刷新资源检测
        /// </summary>
        public static void FlushMonitor(string title, bool number = false)
        {
            if (!LogMonitor)
            {
                return;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }

            item.Flush(title, number);
        }
        /// <summary>
        /// 刷新资源检测
        /// </summary>
        public static void FlushMonitor(string fmt, params object[] args)
        {
            if (!LogMonitor)
            {
                return;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }
            item.Flush(string.Format(fmt, args));
        }
        /// <summary>
        /// 刷新资源检测
        /// </summary>
        public static void EndMonitor()
        {
            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }
            IocScope.Dependency.Remove<MonitorItem>();
            var log = item.End();
            if (log != null)
            {
                var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Monitor");
                Logger.LogInformation(eventId, log);
            }
        }
    }
}