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
    public class DependencyScope : IDisposable
    {
        readonly bool isNew;
        /// <summary>
        /// 生成一个范围
        /// </summary>
        /// <returns></returns>
        private DependencyScope(string name = null)
        {
            if (Local.Value == null)
            {
                isNew = true;
                Local.Value = new ScopeData
                {
                    Name = name ?? "Scope",
                    Scope = this,
                    ServiceScope = DependencyHelper.ServiceScopeFactory.CreateScope()
                };
            }
            else if(Local.Value.ServiceScope == null)
            {
                isNew = true;
                Local.Value.Scope = this;
                Local.Value.Name = name ?? "Scope";
                Local.Value.ServiceScope = DependencyHelper.ServiceScopeFactory.CreateScope();
            }
        }

        /// <summary>
        /// 生成一个范围
        /// </summary>
        /// <returns></returns>
        public static DependencyScope CreateScope(string name = null)
        {
            return new DependencyScope(name);
        }

        /// <summary>
        /// 活动实例
        /// </summary>
        public static readonly AsyncLocal<ScopeData> Local = new AsyncLocal<ScopeData>();

        /// <summary>
        /// 内容
        /// </summary>
        public static ScopeData Value => Local.Value;

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

        #region 清理资源

        // 检测冗余调用
        private bool disposedValue = false;

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;

            if (!isNew)
                return;
            if (!(Local.Value is ScopeData data))
                return;
            Local.Value = null;
            try
            {
                data.ServiceScope?.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            foreach (var func in data.DisposeFunc)
            {
                try
                {
                    func();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            foreach (var den in data.Dependency.Dictionary.Values)
            {
                try
                {
                    if (den is IDisposable disposable)
                        disposable.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion
    }
}