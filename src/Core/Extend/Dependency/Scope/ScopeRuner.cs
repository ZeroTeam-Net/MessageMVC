using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;

namespace Agebull.Common.Ioc
{

    /// <summary>
    /// IOC范围执行器,内部框架使用
    /// </summary>
    public static class ScopeRuner
    {
        /// <summary>
        /// 增加引用计数
        /// </summary>
        /// <returns></returns>
        public static IDisposable AddReferenct()
        {
            var local = scopeLocal.Value;
            if (local != null)
                local.Referenct++;
            return local;
        }

        /// <summary>
        /// 析构本地
        /// </summary>
        public static void DisposeLocal()
        {
            scopeLocal?.Value?.Dispose();
        }

        /// <summary>
        /// 活动实例
        /// </summary>
        static readonly AsyncLocal<ScopeInnerData> scopeLocal = new AsyncLocal<ScopeInnerData>();

        /// <summary>
        /// 范围名称
        /// </summary>
        public static string Name => scopeLocal.Value?.Name;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger ScopeLogger => scopeLocal.Value?.Logger ?? DependencyHelper.Logger;

        /// <summary>
        /// 上下文
        /// </summary>
        internal static IScopeContext ScopeContext
        {
            get => scopeLocal.Value?.Context;
            set => scopeLocal.Value.Context = value;
        }

        /// <summary>
        /// 用户
        /// </summary>
        internal static IUser ScopeUser
        {
            get => scopeLocal.Value?.User;
            set => scopeLocal.Value.User = value;
        }

        /// <summary>
        /// 是否在一个范围内
        /// </summary>
        public static bool InScope => scopeLocal.Value != null;

        /// <summary>
        /// 日志监控节点
        /// </summary>
        internal static MonitorStack MonitorItem
        {
            get => scopeLocal.Value?.MonitorItem;
            set => scopeLocal.Value.MonitorItem = value;
        }

        /// <summary>
        /// 附件内容
        /// </summary>
        internal static ScopeAttachData ScopeDependency => scopeLocal.Value?.ScopeData;

        /// <summary>
        /// 依赖服务范围
        /// </summary>
        internal static IServiceScope ServiceScope => scopeLocal.Value?.ServiceScope;

        /// <summary>
        /// 析构方法
        /// </summary>
        public static List<Action> ScopeDisposeFunc => scopeLocal.Value?.DisposeFunc;

        #region 基础

        static void Run(ContextCallback callback, object state = null)
        {
            var ctx = Thread.CurrentThread.ExecutionContext.CreateCopy();
            ExecutionContext.Run(ctx, callback, state);
        }
        class ScopeBase
        {
            protected readonly ScopeInnerData data;
            internal ScopeBase(string name, ContextInheritType contextInherit)
            {
                data = new ScopeInnerData
                {
                    Name = name,
                    Referenct = 1
                };
                var val = scopeLocal.Value;
                if (val != null)
                    switch (contextInherit)
                    {
                        case ContextInheritType.Clone:
                            if (val.Context != null)
                                data.Context = val.Context.Clone();
                            data.User = val.User;
                            if (val.ScopeData != null)
                                data.ScopeData = val.ScopeData.Clone();
                            break;
                        case ContextInheritType.Sharp:
                            data.Context = val.Context;
                            data.User = val.User;
                            data.ScopeData = val.ScopeData;
                            break;
                    }
            }
            internal void Begin()
            {
                data.ServiceScope = DependencyHelper.ServiceScopeFactory.CreateScope();
                data.Logger = DependencyHelper.LoggerFactory.CreateLogger(data.Name);
                scopeLocal.Value = data;
            }
            internal void End()
            {
                data.Dispose();
            }
        }
        #endregion
        #region 无参方法

        /// <summary>
        /// 在一个依赖范围中运行
        /// </summary>
        /// <param name="name">依赖命名</param>
        /// <param name="action">执行方法</param>
        /// <param name="contextInherit">上下文继承方式</param>
        public static void RunScope(string name, Action action, ContextInheritType contextInherit = ContextInheritType.Clone)
        {
            var scope = new Scope(name ?? "DependencyRun", action, contextInherit);
            Run(scope.Callback);
        }

        /// <summary>
        /// 在一个依赖范围中运行
        /// </summary>
        /// <param name="name">依赖命名</param>
        /// <param name="action">执行方法</param>
        /// <param name="contextInherit">上下文继承方式</param>
        public static void RunScope(string name, Func<Task> action, ContextInheritType contextInherit = ContextInheritType.Clone)
        {
            var scope = new TaskScope(name ?? "DependencyRun", action, contextInherit);
            Run(scope.Callback);
        }

        class Scope : ScopeBase
        {
            readonly Action action;
            internal Scope(string name, Action callback, ContextInheritType contextInherit)
                : base(name, contextInherit)
            {
                action = callback;
            }
            internal void Callback(object _)
            {
                Begin();
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    ScopeRuner.ScopeLogger.Exception(ex, Name);
                }
                finally
                {
                    End();
                }
            }
        }

        /// <summary>
        /// 异步范围
        /// </summary>
        class TaskScope : ScopeBase
        {
            readonly Func<Task> action;
            internal TaskScope(string name, Func<Task> func, ContextInheritType contextInherit)
                : base(name, contextInherit)
            {
                action = func;
            }
            internal async void Callback(object _)
            {
                Begin();
                await Task.Yield();
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    ScopeRuner.ScopeLogger.Exception(ex, Name);
                }
                finally
                {
                    End();
                }
            }
        }
        #endregion

        #region 单参方法

        /// <summary>
        /// 在一个依赖范围中运行
        /// </summary>
        /// <param name="name">依赖命名</param>
        /// <param name="action">执行方法</param>
        /// <param name="arg">执行参数</param>
        /// <param name="contextInherit">上下文继承方式</param>
        public static void RunScope<T>(string name, Action<T> action, T arg, ContextInheritType contextInherit = ContextInheritType.Clone)
        {
            var scope = new Scope<T>(name ?? "DependencyRun", action, contextInherit);
            Run(scope.Callback, arg);
        }

        /// <summary>
        /// 在一个依赖范围中运行
        /// </summary>
        /// <param name="name">依赖命名</param>
        /// <param name="arg">执行参数</param>
        /// <param name="action">执行方法</param>
        /// <param name="contextInherit">上下文继承方式</param>
        public static void RunScope<T>(string name, Func<T, Task> action, T arg, ContextInheritType contextInherit = ContextInheritType.Clone)
        {
            var scope = new TaskScope<T>(name ?? "DependencyRun", action, contextInherit);
            Run(scope.Callback, arg);
        }

        class Scope<T> : ScopeBase
        {
            readonly Action<T> action;
            internal Scope(string name, Action<T> callback, ContextInheritType contextInherit)
                : base(name, contextInherit)
            {
                action = callback;
            }
            internal void Callback(object state)
            {
                Begin();
                try
                {
                    action((T)state);
                }
                catch (Exception ex)
                {
                    ScopeRuner.ScopeLogger.Exception(ex, Name);
                }
                finally
                {
                    End();
                }
            }
        }

        /// <summary>
        /// 异步范围
        /// </summary>
        class TaskScope<T> : ScopeBase
        {
            readonly Func<T, Task> action;
            internal TaskScope(string name, Func<T, Task> func, ContextInheritType contextInherit)
                : base(name, contextInherit)
            {
                action = func;
            }
            internal async void Callback(object state)
            {
                Begin();
                await Task.Yield();
                try
                {
                    await action((T)state);
                }
                catch (Exception ex)
                {
                    ScopeRuner.ScopeLogger.Exception(ex, Name);
                }
                finally
                {
                    End();
                }
            }
        }
        #endregion
    }
}