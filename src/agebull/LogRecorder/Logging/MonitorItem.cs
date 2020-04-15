using System;
using System.Text;

namespace Agebull.Common.Logging
{
    /// <summary>
    ///     跟踪信息
    /// </summary>
    [Serializable]
    internal class MonitorItem
    {
        /// <summary>
        ///     记录堆栈
        /// </summary>
        internal LogStack Stack = new LogStack();

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
        internal void BeginStep(string title)
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
        public void End()
        {
            if (!InMonitor)
                return;
            InMonitor = false;
            while (!Stack.IsEmpty)
            {
                Stack.Pop();
            }
            Stack.FixValue.End = DateTime.Now;
        }

        internal void Trace(string msg)
        {
            Stack.Current.Children.Add(new TraceItem
            {
                Message = msg
            });
        }

    }
}