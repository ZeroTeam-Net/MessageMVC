using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// 范围数据
    /// </summary>
    internal class ScopeData
    {
        /// <summary>
        /// 依赖服务范围
        /// </summary>
        public IServiceScope ServiceScope { get; set; }

        /// <summary>
        /// 范围名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        private ILogger logger;
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger
        {
            get => logger ??= DependencyHelper.LoggerFactory.CreateLogger(Name ?? "Scope");
            set => logger = value;
        }

        /// <summary>
        /// 内部模式,框架使用
        /// </summary>
        public bool InnerModel
        {
            get;
            set;
        }

        /// <summary>
        /// 存储上下文对象,框架使用
        /// </summary>
        public object Context
        {
            get;
            set;
        }

        /// <summary>
        /// 析构方法
        /// </summary>
        public List<Action> DisposeFunc = new List<Action>();

        /// <summary>
        /// 附件内容
        /// </summary>
        public DependencyObjects Dependency = new DependencyObjects();



        #region 清理资源

        // 检测冗余调用
        private bool disposedValue = false;

        ///<inheritdoc/>
        internal void Dispose()
        {
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;
            foreach (var func in DisposeFunc)
            {
                try
                {
                    func();
                }
                catch (Exception e)
                {
                    Logger.Exception(e);
                }
            }
            foreach (var den in Dependency.Dictionary.Values)
            {
                try
                {
                    if (den is IDisposable disposable)
                        disposable.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Exception(e);
                }
            }
            try
            {
                ServiceScope?.Dispose();
            }
            catch (Exception e)
            {
                Logger.Exception(e);
            }
            ServiceScope = null;
        }

        #endregion
    }
}