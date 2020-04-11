using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Base;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LocalValueType = System.Tuple<string, Agebull.EntityModel.Common.DependencyObjects, System.Collections.Generic.List<System.Action>, Microsoft.Extensions.Logging.ILogger>;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// IOC��Χ����,�ڲ����ʹ��
    /// </summary>
    public class IocScope : ScopeBase
    {
        private IServiceScope _scope;
        /// <summary>
        /// ����һ����Χ
        /// </summary>
        /// <returns></returns>
        private IocScope(string name = null)
        {
            _scope = IocHelper.CreateScope();
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
            return new IocScope(name);
        }

        /// <summary>
        /// ������Դ
        /// </summary>
        protected override void OnDispose()
        {
            if (DisposeFunc != null)
            {
                foreach (var func in DisposeFunc)
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
                Local.Value = null;
            }
            try
            {
                IocHelper.DisposeScope();
                _scope.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            GC.Collect();
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
    }
    /// <summary>
    /// ��Χ����
    /// </summary>
    public class ScopeData
    {
        string name;
        /// <summary>
        /// ��Χ����
        /// </summary>
        public string Name
        {
            set
            {
                name = value;
                Logger = IocHelper.LoggerFactory.CreateLogger(Name);
            }
            get => name;
        }

        /// <summary>
        /// ��־��¼��
        /// </summary>
        public ILogger Logger
        {
            get;
            set;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public List<Action> DisposeFunc = new List<Action>();

        /// <summary>
        /// ��������
        /// </summary>
        public DependencyObjects Dependency = new DependencyObjects();
    }
}