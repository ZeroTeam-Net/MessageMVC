using System;
using System.Collections.Generic;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// 范围数据
    /// </summary>
    public class ScopeData
    {
        /// <summary>
        /// 依赖服务范围
        /// </summary>
        internal IServiceScope ServiceScope { get;  set; }

        /// <summary>
        /// 当前范围
        /// </summary>
        public IDisposable Scope { get; internal set; }

        string name;
        /// <summary>
        /// 范围名称
        /// </summary>
        public string Name
        {
            set
            {
                name = value;
                Logger = DependencyHelper.LoggerFactory.CreateLogger(Name);
            }
            get => name;
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
        /// 内部模式,框架使用
        /// </summary>
        public bool InnerModel
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
    }
}