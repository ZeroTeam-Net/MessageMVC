using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// IOC��Χ����,�ڲ����ʹ��
    /// </summary>
    public class DependencyScope : IDisposable
    {
        readonly bool isNew;
        /// <summary>
        /// ����һ����Χ
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
        /// ����һ����Χ
        /// </summary>
        /// <returns></returns>
        public static DependencyScope CreateScope(string name = null)
        {
            return new DependencyScope(name);
        }

        /// <summary>
        /// �ʵ��
        /// </summary>
        public static readonly AsyncLocal<ScopeData> Local = new AsyncLocal<ScopeData>();

        /// <summary>
        /// ����
        /// </summary>
        public static ScopeData Value => Local.Value;

        /// <summary>
        /// ��Χ����
        /// </summary>
        public static string Name => Local.Value?.Name;

        /// <summary>
        /// ��־��¼��
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
        /// ��������
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
        /// ��������Χ
        /// </summary>
        public static IServiceScope ServiceScope => Local.Value?.ServiceScope;

        /// <summary>
        /// �ڲ�ģʽ,���ʹ��
        /// </summary>
        public static bool InnerModel => Local.Value?.InnerModel ?? false;

        /// <summary>
        /// ��������
        /// </summary>
        public static List<Action> DisposeFunc => Local.Value?.DisposeFunc;

        #region ������Դ

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