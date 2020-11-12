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
        bool isNew;
        /// <summary>
        /// 生成一个范围
        /// </summary>
        /// <returns></returns>
        private DependencyScope(string name = null)
        {
            isNew = Local.Value == null;
            if (isNew)
                Local.Value = new ScopeData
                {
                    Name = name ?? "Scope",
                    Scope = this,
                    ServiceScope = DependencyHelper.ServiceScopeFactory.CreateScope()
                };
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
        public static ScopeData LocalValue => Local.Value ??= new ScopeData { Name = "Scope" };

        /// <summary>
        /// 内容
        /// </summary>
        public static ScopeData Value
        {
            get => Local.Value;
            set => Local.Value = value;
        }

        /// <summary>
        /// 范围名称
        /// </summary>
        public static string Name
        {
            get => LocalValue.Name;
            set => LocalValue.Name = value;
        }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger Logger
        {
            get => LocalValue.Logger;
            set => LocalValue.Logger = value;
        }

        /// <summary>
        /// 依赖服务范围
        /// </summary>
        public static IServiceScope ServiceScope => Local.Value?.ServiceScope;

        /// <summary>
        /// 内部模式,框架使用
        /// </summary>
        public static bool InnerModel
        {
            get => LocalValue.InnerModel;
            set => LocalValue.InnerModel = value;
        }

        /// <summary>
        /// 析构方法
        /// </summary>
        public static List<Action> DisposeFunc => LocalValue.DisposeFunc;

        /// <summary>
        /// 附件内容
        /// </summary>
        public static DependencyObjects Dependency => LocalValue.Dependency;

        #region 清理资源

        void DoDisposeAction()
        {
            if (!isNew)
                return;
            if (!(Local.Value is ScopeData data))
                return;
            Local.Value = null;
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

            DoDisposeAction();
            try
            {
                ServiceScope?.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}