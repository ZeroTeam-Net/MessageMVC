using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Base;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Agebull.Common.Ioc
{
    /// <summary>
    /// IOC范围对象,内部框架使用
    /// </summary>
    public class DependencyScope : ScopeBase
    {
        private IServiceScope _scope;
        /// <summary>
        /// 生成一个范围
        /// </summary>
        /// <returns></returns>
        private DependencyScope(string name = null)
        {
            _scope = DependencyHelper.CreateScope();
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
        /// 清理资源
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
                DependencyHelper.DisposeScope();
                _scope.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            GC.Collect();
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
    }
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