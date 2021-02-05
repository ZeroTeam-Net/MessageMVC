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
            var sec = ConfigurationHelper.GetSection("Logging:LogLevel:MessageMVC");
            if (sec != null && Enum.TryParse<LogLevel>(sec.Value, out var l))
            {
                Level = l;
            }
            else
            {
                Level = LogLevel.Error;
            }
        }

        /// <summary>
        /// 当前日志级别
        /// </summary>
        public static LogLevel Level { get; set; }

        /// <summary>
        /// 是否启动跟踪日志
        /// </summary>
        public static bool LogMonitor => Level <= LogLevel.Information && ScopeRuner.InScope;

        /// <summary>
        /// 是否启动跟踪日志
        /// </summary>
        public static bool LogMonitorInformation => Level <= LogLevel.Information && ScopeRuner.InScope;

        /// <summary>
        /// 跟踪日志是否包含详细信息
        /// </summary>
        public static bool LogMonitorDebug => Level <= LogLevel.Debug && ScopeRuner.InScope;

        /// <summary>
        /// 跟踪日志是否包含详细信息
        /// </summary>
        public static bool LogMonitorTrace => Level <= LogLevel.Trace && ScopeRuner.InScope;

        /// <summary>
        /// 当前范围数据
        /// </summary>
        static MonitorStack MonitorItem => ScopeRuner.MonitorItem;

        #endregion

        #region 跟踪输出

        /// <summary>
        /// 开始监视日志
        /// </summary>
        [Conditional("monitor")]
        public static void BeginMonitor(string title)
        {
            if (!LogMonitor)
            {
                return;
            }
            var item = new MonitorStack();
            item.BeginMonitor(title ?? ScopeRuner.Name);
            ScopeRuner.MonitorItem = item;
        }
        /// <summary>
        /// 开始监视日志
        /// </summary>
        [Conditional("monitor")]
        public static void BeginMonitor(Func<string> title)
        {
            if (!LogMonitor)
            {
                return;
            }
            var item = new MonitorStack();
            item.BeginMonitor(title() ?? ScopeRuner.Name);
            ScopeRuner.MonitorItem = item;
        }

        /// <summary>
        /// 开始监视日志
        /// </summary>
        [Conditional("monitor")]
        public static void BeginMonitor(TraceStep fix)
        {
            if (!LogMonitor)
            {
                return;
            }
            var item = new MonitorStack();
            item.BeginMonitor(fix);
            ScopeRuner.MonitorItem = item;
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
        public static void BeginTraceStepMonitor(string title)
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

            MonitorItem.BeginStep(title);
        }

        /// <summary>
        /// 开始监视日志步骤
        /// </summary>
        [Conditional("monitor")]
        public static void BeginTraceStepMonitor(Func<string> func)
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

            MonitorItem.BeginStep(func());
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
        public static void MonitorDebug(string message)
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
            if (!LogMonitorDebug)
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
        public static TraceStep EndMonitor(bool clear = true)
        {
            var item = MonitorItem;
            if (item == null || !item.InMonitor)
            {
                return null;
            }
            if (clear)
                ScopeRuner.MonitorItem = null;
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
            if (root == null || !logger.IsEnabled(LogLevel.Information))
                return;
            var texter = new StringBuilder();
            Message(texter, root.Start, root);
            logger.Information(texter.ToString());
        }

        /// <summary>
        /// 结束监视日志
        /// </summary>
        public static string TraceMonitor(TraceStep root)
        {
            if (root == null)
                return null;
            var texter = new StringBuilder();
            Message(texter, root.Start, root);
            return texter.ToString();
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

        #region Scope

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IDisposable MonitorScope(string title = null, ILogger logger = null) => new MonitorInnerScope(title, logger);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IDisposable MonitorScope(Func<string> title, ILogger logger = null) => new MonitorInnerScope(title, logger);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static IDisposable StepScope(string title) => new FlowStepScope(title);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static IDisposable TraceStepScope(string title) => new FlowTracerStepScope(title);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static IDisposable DebugStepScope(string title) => new FlowTracerDebugStepScope(title);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static IDisposable StepScope(Func<string> title) => new FlowStepScope(title);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static IDisposable TraceStepScope(Func<string> title) => new FlowTracerStepScope(title);

        /// <summary>
        /// 步骤范围
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static IDisposable DebugStepScope(Func<string> title) => new FlowTracerDebugStepScope(title);

        /// <summary>
        /// 流程跟踪步骤范围
        /// </summary>
        internal class MonitorInnerScope : IDisposable
        {
            readonly ILogger _logger;
            internal MonitorInnerScope(string title, ILogger logger)
            {
                _logger = logger ?? ScopeRuner.ScopeLogger;
                BeginMonitor(title);
            }
            internal MonitorInnerScope(Func<string> title, ILogger logger)
            {
                _logger = logger ?? ScopeRuner.ScopeLogger;
                BeginMonitor(title);
            }

            /// <inheritdoc/>
            void IDisposable.Dispose()
            {
                var item = EndMonitor();
                _logger.Information(item.Message);
            }

        }

        /// <summary>
        /// 流程跟踪步骤范围
        /// </summary>
        internal class FlowStepScope : IDisposable
        {
            internal FlowStepScope(string title) => BeginStepMonitor(title);

            internal FlowStepScope(Func<string> title) => BeginStepMonitor(title);

            /// <inheritdoc/>
            void IDisposable.Dispose() => EndStepMonitor();

        }

        /// <summary>
        /// 流程跟踪步骤范围
        /// </summary>
        internal class FlowTracerStepScope : IDisposable
        {
            internal FlowTracerStepScope(string title) => BeginTraceStepMonitor(title);

            internal FlowTracerStepScope(Func<string> title) => BeginTraceStepMonitor(title);

            /// <inheritdoc/>
            void IDisposable.Dispose() => EndStepMonitor();

        }


        /// <summary>
        /// 流程跟踪调试步骤范围
        /// </summary>
        internal class FlowTracerDebugStepScope : IDisposable
        {
            internal FlowTracerDebugStepScope(string title) => BeginDebugStepMonitor(title);
            internal FlowTracerDebugStepScope(Func<string> title) => BeginDebugStepMonitor(title);

            /// <inheritdoc/>
            void IDisposable.Dispose() => EndDebugStepMonitor();

        }

        #endregion
    }
}