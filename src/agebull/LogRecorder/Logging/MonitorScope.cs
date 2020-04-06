using Agebull.Common.Base;
using System;

namespace Agebull.Common.Logging
{

    /// <summary>
    /// 根据步骤范围
    /// </summary>
    public class MonitorScope : ScopeBase
    {
        private bool _isStep;
        /// <summary>
        /// 生成范围
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDisposable CreateScope(string name)
        {
            if (!LogRecorder.LogMonitor)
            {
                return new EmptyScope();
            }

            var scope = new MonitorScope
            {
                _isStep = LogRecorder.MonitorItem?.InMonitor == true
            };
            if (scope._isStep)
            {
                LogRecorder.BeginStepMonitor(name);
            }
            else
            {
                //IocScope.Logger = IocHelper.LoggerFactory.CreateLogger(name);
                LogRecorder.BeginMonitor(name);
            }
            return scope;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnDispose()
        {
            if (_isStep)
            {
                LogRecorder.EndStepMonitor();
            }
            else
            {
                LogRecorder.EndMonitor();
            }
        }
    }

    /// <summary>
    /// 根据步骤范围
    /// </summary>
    public class MonitorStepScope : ScopeBase
    {
        /// <summary>
        /// 生成范围
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IDisposable CreateScope(string name, params object[] args)
        {
            if (!LogRecorder.LogMonitor || LogRecorder.MonitorItem?.InMonitor != true)
            {
                return new EmptyScope();
            }
            var title = args == null || args.Length == 0
                ? name
                : string.Format(name, args);
            var scope = new MonitorStepScope();
            LogRecorder.BeginStepMonitor(title);
            return scope;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnDispose()
        {
            LogRecorder.EndStepMonitor();
        }
    }
}