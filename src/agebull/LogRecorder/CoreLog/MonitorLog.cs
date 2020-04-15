using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
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
        internal static MonitorItem MonitorItem => DependencyScope.Dependency.Dependency<MonitorItem>();

        /// <summary>
        /// 开始检测资源
        /// </summary>
        [Conditional("LogMonitor")]
        public static void BeginMonitor(string title)
        {
            if (!LogMonitor || !Logger.IsEnabled(LogLevel.Information))
            {
                return;
            }
            var item = new MonitorItem();
            item.BeginMonitor(title);
            DependencyScope.Dependency.Annex(item);
        }

        /// <summary>
        /// 开始监视日志步骤
        /// </summary>
        [Conditional("LogMonitor")]
        public static void BeginStepMonitor(string title)
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

            MonitorItem.BeginStep(title);
        }

        /// <summary>
        /// 结束监视日志步骤
        /// </summary>
        [Conditional("LogMonitor")]
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
        /// 加入监视跟踪
        /// </summary>
        [Conditional("LogMonitor")]
        public static void MonitorTrace(string message)
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

            item.Trace(message);
            return;
        }

        /// <summary>
        /// 加入监视跟踪
        /// </summary>
        [Conditional("LogMonitor")]
        public static void MonitorTrace(string message, params object[] args)
        {
            if (!LogMonitor || message == null)
            {
                return;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }

            item.Trace(string.Format(message, args));
            return;
        }

        /// <summary>
        /// 加入监视跟踪
        /// </summary>
        [Conditional("LogMonitor")]
        public static void MonitorTrace(Func<string> message)
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

            item.Trace(message());
            return;
        }

        /// <summary>
        /// 结束监视日志
        /// </summary>
        public static TraceStep EndMonitor()
        {
            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return null;
            }
            DependencyScope.Dependency.Remove<MonitorItem>();
            item.End();
            return item.Stack.FixValue;
        }

        #region 表格输出

        /// <summary>
        /// 结束监视日志
        /// </summary>
        public static void TraceMonitor(TraceStep root)
        {
            var texter = new StringBuilder();
            Message(texter, root.Start, root);
            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Monitor");
            Logger.LogInformation(eventId, texter.ToString());
        }

        /// <summary>
        ///     刷新资源检测
        /// </summary>
        static void Message(StringBuilder text, DateTime start, TraceStep step, int level = 0)
        {
            Write(text, ItemType.Begin, level, step.Message,
                $"|开始| {step.Start:HH:mm:ss.ffff} |");
            foreach (var item in step.Children)
            {
                if (item is TraceStep traceStep)
                {
                    Message(text, start, traceStep, level + 1);
                }
                else
                {
                    Write(text, ItemType.Item, level + 1, item.Message, null);
                }
            }
            Write(text, ItemType.End, level, step.Message,
                $"|完成| {step.End:HH:mm:ss.ffff} |{(step.End - step.Start).TotalMilliseconds.ToFixLenString(7, 1)}/{(step.End - start).TotalMilliseconds.ToFixLenString(7, 1)}|");

        }
        internal enum ItemType
        {
            Begin,
            Item,
            End
        }

        static void Write(StringBuilder texter, ItemType type, int level, string title, string coll)
        {
            texter.AppendLine();

            switch (type)
            {
                case ItemType.Begin:
                    if (level > 0)
                    {
                        texter.Append('│', level - 1);
                        texter.Append("├┬");
                    }
                    else
                    {
                        texter.Append('┌');
                    }
                    break;
                case ItemType.End:
                    if (level > 0)
                    {
                        texter.Append('│', level);
                    }

                    texter.Append('└');
                    break;
                default:
                    if (level > 0)
                    {
                        texter.Append('│', level);
                    }

                    texter.Append('├');
                    level++;
                    break;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = "*";
            }

            texter.Append(title);
            if (coll == null)
            {
                return;
            }
            var l = level + title.GetLen();
            if (l < 50)
            {
                texter.Append(' ', 50 - l);
            }
            texter.Append(coll);
        }
        #endregion
    }
}