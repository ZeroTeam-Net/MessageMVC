using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Base;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// IOC范围对象,内部框架使用
    /// </summary>
    public class DependencyScope : IDisposable, IAsyncDisposable
    {
        private readonly IServiceScope scope;
        private readonly ServiceProvider provider;
        /// <summary>
        /// 生成一个范围
        /// </summary>
        /// <returns></returns>
        private DependencyScope(string name = null)
        {
            provider = DependencyHelper.ServiceCollection.BuildServiceProvider(true);
            scope = provider.GetService<IServiceScopeFactory>().CreateScope();

            Local.Value = new ScopeData
            {
                Name = name ?? "Scope"
            };
        }

        /// <summary>
        /// 生成一个范围
        /// </summary>
        /// <returns></returns>
        public static IDisposable CreateScope(string name = null)
        {
            return new DependencyScope(name);
        }

        /// <summary>
        /// 活动实例
        /// </summary>
        internal static readonly AsyncLocal<ScopeData> Local = new AsyncLocal<ScopeData>();

        /// <summary>
        /// 析构方法
        /// </summary>
        static ScopeData LocalValue => Local.Value ??= new ScopeData { Name = "Scope" };

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
            foreach(var den in data.Dependency._dictionary.Values)
            {
                if (den is IDisposable disposable)
                    disposable.Dispose();
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
                scope.Dispose();
                provider.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            GC.Collect();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;

            DoDisposeAction();
            try
            {
                scope.Dispose();
                await provider.DisposeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            GC.Collect();
        }
        #endregion
    }
}