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
    /// IOC��Χ����,�ڲ����ʹ��
    /// </summary>
    public class DependencyScope : IDisposable, IAsyncDisposable
    {
        private readonly IServiceScope scope;
        private readonly ServiceProvider provider;
        /// <summary>
        /// ����һ����Χ
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
        /// ����һ����Χ
        /// </summary>
        /// <returns></returns>
        public static IDisposable CreateScope(string name = null)
        {
            return new DependencyScope(name);
        }

        /// <summary>
        /// �ʵ��
        /// </summary>
        internal static readonly AsyncLocal<ScopeData> Local = new AsyncLocal<ScopeData>();

        /// <summary>
        /// ��������
        /// </summary>
        static ScopeData LocalValue => Local.Value ??= new ScopeData { Name = "Scope" };

        /// <summary>
        /// ��Χ����
        /// </summary>
        public static string Name
        {
            get => LocalValue.Name;
            set => LocalValue.Name = value;
        }

        /// <summary>
        /// ��־��¼��
        /// </summary>
        public static ILogger Logger
        {
            get => LocalValue.Logger;
            set => LocalValue.Logger = value;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public static List<Action> DisposeFunc => LocalValue.DisposeFunc;

        /// <summary>
        /// ��������
        /// </summary>
        public static DependencyObjects Dependency => LocalValue.Dependency;

        #region ������Դ

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
        // ����������
        private bool disposedValue = false;

        // ��Ӵ˴�������ȷʵ�ֿɴ���ģʽ��
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