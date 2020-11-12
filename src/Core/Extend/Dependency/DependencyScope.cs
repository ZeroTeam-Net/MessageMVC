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
        bool isNew;
        /// <summary>
        /// ����һ����Χ
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
        public static ScopeData LocalValue => Local.Value ??= new ScopeData { Name = "Scope" };

        /// <summary>
        /// ����
        /// </summary>
        public static ScopeData Value
        {
            get => Local.Value;
            set => Local.Value = value;
        }

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
        /// ��������Χ
        /// </summary>
        public static IServiceScope ServiceScope => Local.Value?.ServiceScope;

        /// <summary>
        /// �ڲ�ģʽ,���ʹ��
        /// </summary>
        public static bool InnerModel
        {
            get => LocalValue.InnerModel;
            set => LocalValue.InnerModel = value;
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