using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Context;

namespace Agebull.Common.Ioc
{
    /// <summary>
    /// 依赖范围框架数据
    /// </summary>
    internal class ScopeInnerData : IDisposable
    {
        public ScopeInnerData()
        {
            Id = RandomCode.Generate(16);
        }
        /// <summary>
        /// 引用计数，归0时析构
        /// </summary>
        public int Referenct { get; set; }

        /// <summary>
        /// 依赖服务范围
        /// </summary>
        public IServiceScope ServiceScope { get; set; }

        /// <summary>
        /// 唯一标准
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// 范围名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILogger Logger
        {
            get;
            set;
        }

        /// <summary>
        /// 日志监控节点
        /// </summary>
        public MonitorStack MonitorItem
        {
            get;
            set;
        }

        // 存储上下文对象
        private IScopeContext context;

        /// <summary>
        /// 存储上下文对象,框架使用
        /// </summary>
        public IScopeContext Context
        {
            get => context;
            set
            {
                context = value;
                if (value != null)
                    value.ScopeData = ScopeData;
            }
        }

        /// <summary>
        /// 存储上下文对象,框架使用
        /// </summary>
        public IUser User
        {
            get;
            set;
        }

        /// <summary>
        /// 析构方法
        /// </summary>
        public readonly List<Action> DisposeFunc = new List<Action>();

        /// <summary>
        /// 附件内容
        /// </summary>
        public ScopeAttachData ScopeData = new ScopeAttachData();

        #region 清理资源

        ///<inheritdoc/>
        public void Dispose()
        {
            if (Referenct == 0)
            {
                return;
            }
            var logger = Logger;
            if (--Referenct > 0)
            {
                logger.Trace(() => $"{Name}:范围计数不为1，未析构");
                return;
            }

            foreach (var func in DisposeFunc)
            {
                try
                {
                    func();
                }
                catch (Exception e)
                {
                    logger.Exception(e);
                }
            }
            foreach (var den in ScopeData.dictionary.Values)
            {
                try
                {
                    if (den is IDisposable disposable)
                        disposable.Dispose();
                }
                catch (Exception e)
                {
                    logger.Exception(e);
                }
            }
            var scope = ServiceScope;
            ServiceScope = null;
            try
            {
                scope?.Dispose();
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            if (MonitorItem != null)
                logger.TraceMonitor(MonitorItem.Stack.FixValue);
            logger.Trace(() => $"{Name}:范围已析构");
        }
        #endregion

    }
}