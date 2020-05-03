using System;
using System.Text;

namespace Agebull.Common.Logging
{
    /// <summary>
    ///     跟踪信息
    /// </summary>
    [Serializable]
    public class MonitorItem
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
}