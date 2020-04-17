using System;
using System.Collections.Generic;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// 范围数据
    /// </summary>
    public class ScopeData
    {
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
        /// 析构方法
        /// </summary>
        public List<Action> DisposeFunc = new List<Action>();

        /// <summary>
        /// 附件内容
        /// </summary>
        public DependencyObjects Dependency = new DependencyObjects();
    }
}