using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// IOC范围对象,内部框架使用
    /// </summary>
    public static class DependencyRun
    {
        /// <summary>
        /// 运行在一个依赖范围，析构请调用DependencyRun.DisposeLocal
        /// </summary>
        /// <returns></returns>
        public static void RunScope(string name, ContextCallback callback, object state = null)
        {
            Local.Value = new ScopeData
            {
                Name = name ?? "Scope",
                ServiceScope = DependencyHelper.ServiceScopeFactory.CreateScope()
            };
            using var ctx = Thread.CurrentThread.ExecutionContext.CreateCopy();
            ExecutionContext.Run(ctx, callback, state);
        }

        /// <summary>
        /// 析构本地
        /// </summary>
        public static void DisposeLocal()
        {
            Local?.Value?.Dispose();
        }

        /// <summary>
        /// 活动实例
        /// </summary>
        static readonly AsyncLocal<ScopeData> Local = new AsyncLocal<ScopeData>();

        /// <summary>
        /// 范围名称
        /// </summary>
        public static string Name => Local.Value?.Name;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger Logger
        {
            get
            {
                if (Local.Value == null)
                    Local.Value = new ScopeData();
                return Local.Value.Logger;
            }
        }

        /// <summary>
        /// 上下文
        /// </summary>
        public static object Context
        {
            get => Local.Value.Context;
            set => Local.Value.Context = value;
        }

        /// <summary>
        /// 附件内容
        /// </summary>
        public static DependencyObjects Dependency
        {
            get
            {
                if (Local.Value == null)
                    Local.Value = new ScopeData();
                return Local.Value.Dependency;
            }
        }

        /// <summary>
        /// 依赖服务范围
        /// </summary>
        public static IServiceScope ServiceScope => Local.Value?.ServiceScope;

        /// <summary>
        /// 内部模式,框架使用
        /// </summary>
        public static bool InnerModel => Local.Value?.InnerModel ?? false;

        /// <summary>
        /// 析构方法
        /// </summary>
        public static List<Action> DisposeFunc => Local.Value?.DisposeFunc;

    }
}