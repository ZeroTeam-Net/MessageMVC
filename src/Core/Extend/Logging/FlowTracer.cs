using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
namespace Agebull.Common.Logging
{
    /// <summary>
    ///   流程跟踪器
    /// </summary>
    public static class FlowTracer
    {
        #region Option

        /// <summary>
        ///   静态构造
        /// </summary>
        static FlowTracer()
        {
            ConfigurationHelper.RegistOnChange("Logging", ReadConfig, true);
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        private static void ReadConfig()
        {
            var sec = ConfigurationHelper.Get("Logging:LogLevel");
            if (sec != null && Enum.TryParse<LogLevel>(sec.GetStr("MessageMVC"), out var l))
            {
                Level = l;
            }
            else
            {
                Level = LogLevel.Information;
            }
        }

        /// <summary>
        /// 当前日志级别
        /// </summary>
        public static LogLevel Level { get; set; }

        #endregion

        /// <summary>
        /// 是否启动跟踪日志
        /// </summary>
        public static bool LogMonitor => Level <= LogLevel.Information;

        /// <summary>
        /// 是否启动跟踪日志
        /// </summary>
        public static bool LogMonitorInformation => Level <= LogLevel.Information;

        /// <summary>
        /// 跟踪日志是否包含详细信息
        /// </summary>
        public static bool LogMonitorDebug => Level <= LogLevel.Debug;

        /// <summary>
        /// 跟踪日志是否包含详细信息
        /// </summary>
        public static bool LogMonitorTrace => Level <= LogLevel.Trace;

        /// <summary>
        /// 当前范围数据
        /// </summary>
        static Local MonitorItem => DependencyScope.Dependency.Dependency<Local>();

        #region 跟踪输出

        /// <summary>
        /// 开始检测资源
        /// </summary>
        [Conditional("monitor")]
        public static void BeginMonitor(string title)
        {
            if (!LogMonitor)
            {
                return;
            }
            var item = new Local();
            item.BeginMonitor(title);
            DependencyScope.Dependency.Annex(item);
        }

        /// <summary>
        /// 开始监视日志步骤
        /// </summary>
        [Conditional("monitor")]
        public static void BeginStepMonitor(string title)
        {
            if (!LogMonitorInformation)
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
        /// 开始监视日志步骤
        /// </summary>
        [Conditional("monitor")]
        public static void BeginStepMonitor(Func<string> title)
        {
            if (!LogMonitorInformation)
            {
                return;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }

            MonitorItem.BeginStep(title());
        }
        /// <summary>
        /// 开始监视日志步骤
        /// </summary>
        [Conditional("monitor")]
        public static void BeginDebugStepMonitor(Func<string> func)
        {
            if (!LogMonitorDebug)
            {
                return;
            }

            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return;
            }

            MonitorItem.BeginStep(func());
        }

        /// <summary>
        /// 开始监视日志步骤
        /// </summary>
        [Conditional("monitor")]
        public static void BeginDebugStepMonitor(string title)
        {
            if (!LogMonitorDebug)
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
        [Conditional("monitor")]
        public static void EndDebugStepMonitor()
        {
            if (!LogMonitorDebug)
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
        /// 结束监视日志步骤
        /// </summary>
        [Conditional("monitor")]
        public static void EndStepMonitor()
        {
            if (!LogMonitorInformation)
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
        [Conditional("monitor")]
        public static void MonitorInfomation(string message)
        {
            if (!LogMonitorInformation || message == null)
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
        [Conditional("monitor")]
        public static void MonitorInfomation(string message, params object[] args)
        {
            if (!LogMonitorInformation || message == null)
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
        [Conditional("monitor")]
        public static void MonitorInfomation(Func<string> message)
        {
            if (!LogMonitorInformation)
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
        /// 加入监视跟踪
        /// </summary>
        [Conditional("monitor")]
        public static void MonitorTrace(string message)
        {
            if (!LogMonitorTrace)
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
        [Conditional("monitor")]
        public static void MonitorTrace(Func<string> message)
        {
            if (!LogMonitorTrace)
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
        /// 加入监视跟踪
        /// </summary>
        [Conditional("monitor")]
        public static void MonitorDetails(Func<string> message)
        {
            if (!LogMonitorTrace)
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
        /// 加入监视跟踪
        /// </summary>
        [Conditional("monitor")]
        public static void MonitorDetails(string message)
        {
            if (!LogMonitorDebug)
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
        [Conditional("monitor")]
        public static void MonitorError(Func<string> message)
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
        /// 加入监视跟踪
        /// </summary>
        [Conditional("monitor")]
        public static void MonitorError(Exception ex, string message)
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
            item.Trace($"{message ?? "Exception"}\r\n{ex.Message}");
            return;
        }

        /// <summary>
        /// 加入监视跟踪
        /// </summary>
        [Conditional("monitor")]
        public static void MonitorError(string message)
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
            item.Trace(message);
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
            DependencyScope.Dependency.Remove<Local>();
            item.End();
            return item.Stack.FixValue;
        }
        #endregion
        #region 表格输出

        /// <summary>
        /// 结束监视日志
        /// </summary>
        public static void TraceMonitor(this ILogger logger, TraceStep root)
        {
            if (root == null)
                return;
            var texter = new StringBuilder();
            Message(texter, root.Start, root);
            logger.LogInformation(LoggerExtension.NewEventId("Monitor"), texter.ToString());
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

        #region SUB

        /// <summary>
        ///     跟踪信息
        /// </summary>
        class Local
        {
            /// <summary>
            ///     记录堆栈
            /// </summary>
            internal LocalTraceStack Stack = new LocalTraceStack();

            /// <summary>
            ///     侦测开关
            /// </summary>
            internal bool InMonitor;

            /// <summary>
            ///     开始检测资源
            /// </summary>
            public void BeginMonitor(string title)
            {
                InMonitor = true;
                Stack.SetFix(new TraceStep
                {
                    Message = title
                });
            }

            /// <summary>
            ///     刷新资源检测
            /// </summary>
            public void BeginStep(string title)
            {
                if (InMonitor)
                    Stack.Push(new TraceStep
                    {
                        Message = title
                    });
            }

            /// <summary>
            ///     刷新资源检测
            /// </summary>
            public void EndStep()
            {
                if (InMonitor)
                    Stack.Pop();
            }

            /// <summary>
            ///     刷新资源检测
            /// </summary>
            public TraceStep End()
            {
                if (!InMonitor)
                    return null;
                InMonitor = false;
                while (!Stack.IsEmpty)
                {
                    Stack.Pop();
                }
                Stack.FixValue.End = DateTime.Now;
                return Stack.FixValue;
            }
            /// <summary>
            /// 设置跟踪消息
            /// </summary>
            /// <param name="msg"></param>
            public void Trace(string msg)
            {
                Stack.Current.Children.Add(new TraceItem
                {
                    Message = msg
                });
            }

        }
        #endregion
    }
}