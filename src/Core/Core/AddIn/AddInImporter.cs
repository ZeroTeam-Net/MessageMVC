using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ZeroTeam.MessageMVC.AddIn
{
    /// <summary>
    /// MEF插件导入器
    /// </summary>
    internal class AddInImporter
    {
        #region 程序集排除

        private static readonly HashSet<string> knowAssemblies = new();

        static bool CheckAssembly(string file)
        {
            if (file.IsMissing())
            {
                return false;
            }
            if (knowAssemblies.Contains(file))
            {
                return false;
            }
            knowAssemblies.Add(file);

            var name = Path.GetFileName(file);
            return CheckAssemblyName(name);
        }

        private static bool CheckAssemblyName(string name)
        {
            if (knowAssemblies.Contains(name))
            {
                return false;
            }
            knowAssemblies.Add(name);

            switch (name.Split('.', 2)[0])
            {
                case "System":
                case "Microsoft":
                case "NuGet":
                case "Newtonsoft":
                    return false;
            }

            try
            {
                if (name.IsFirst("netstandard") ||
                    name.IsFirst("BeetleX") ||
                    name.IsFirst("Agebull.Common.") ||
                    name.IsMe("CSRedis") ||
                    name.IsMe("RabbitMQ.Client") ||
                    name.IsMe("Confluent.Kafka") ||
                    name.IsMe("ZeroTeam.MessageMVC.Core") ||
                    name.IsMe("ZeroTeam.MessageMVC.Abstractions") ||
                    name.IsMe("ZeroTeam.MessageMVC.Tools") ||
                    name.IsMe("ZeroTeam.MessageMVC.Consul") ||
                    name.IsMe("ZeroTeam.MessageMVC.ApiContract") ||
                    name.IsMe("ZeroTeam.MessageMVC.Kafka") ||
                    name.IsMe("ZeroTeam.MessageMVC.Tcp") ||
                    name.IsMe("ZeroTeam.MessageMVC.Http") ||
                    name.IsMe("ZeroTeam.MessageMVC.RabbitMQ") ||
                    name.IsMe("ZeroTeam.MessageMVC.RedisMQ") ||
                    name.IsMe("ZeroTeam.MessageMVC.PageInfoAutoSave")
                    )
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        public class AutoRegisterCatalog
        {
            /// <summary>
            /// 插件对象
            /// </summary>
            [ImportMany(typeof(IAutoRegister))]
            public IEnumerable<IAutoRegister> Import { get; set; }

        }

        /// <summary>
        /// 所有自动注册对象
        /// </summary>
        internal List<IAutoRegister> registers = new();

        /// <summary>
        /// 载入插件
        /// </summary>
        internal void LoadAddIn(ILogger logger)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //CheckApplication(logger);
            FindFiles(logger, AppDomain.CurrentDomain.BaseDirectory, true);
            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                return;
            }
            if (!Directory.Exists(ZeroAppOption.Instance.AddInPath))
            {
                logger.Information($"插件地址无效:{ZeroAppOption.Instance.AddInPath}");
                return;
            }
            FindFiles(logger, ZeroAppOption.Instance.AddInPath, false);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine(args.Name);
            var dir = Path.GetDirectoryName(args.RequestingAssembly.Location);
            var file = args.Name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            return Assembly.LoadFile(Path.Combine(dir, file + ".dll"));
        }

        public List<Assembly> Assemblies = new List<Assembly>();

        void FindFiles(ILogger logger, string folder, bool directoryOnly)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }
            logger.Information($"插件路径:{folder}");

            IOHelper.Search(folder, "*.dll", directoryOnly, (path, file) =>
             {
                 if (!CheckAssembly(file))
                     return;
                 logger.Information($"插件路径:{file}");
                // 通过容器对象将宿主和部件组装到一起。 
                try
                 {
                     var catalog = new AutoRegisterCatalog();
                     if (!directoryOnly)
                     {
                         var ass = Assembly.LoadFile(file);
                         Assemblies.Add(ass);
                         using var provider = new AssemblyCatalog(ass);
                         using var c = new CompositionContainer(provider);
                         c.ComposeParts(catalog);
                     }
                     else
                     {
                         using var provider = new DirectoryCatalog(path, Path.GetFileName(file));
                         using var c = new CompositionContainer(provider);
                         c.ComposeParts(catalog);
                     }

                     if (catalog.Import == null || !catalog.Import.Any())
                     {
                         return;
                     }
                     foreach (var obj in catalog.Import)
                     {
                         logger.Information($"发现插件:{obj}");
                         registers.Add(obj);
                     }
                 }
                 catch (Exception e2)
                 {
                     logger.Exception(e2);
                 }
             });
        }


        /// <summary>
        /// 载入插件
        /// </summary>
        internal void AutoRegist(ILogger logger)
        {
            if (registers.Count == 0)
            {
                return;
            }
            foreach (var reg in registers.ToArray())
            {
                try
                {
                    reg.AutoRegist(DependencyHelper.ServiceCollection, logger);
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】基础依赖注册成功");
                }
                catch (System.Exception ex)
                {
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】基础依赖注册异常.{ex.Message}");
                    logger.Exception(ex);
                }
            }
        }

        /// <summary>
        /// 载入插件
        /// </summary>
        internal void LateConfigRegist(ILogger logger)
        {
            if (registers.Count == 0)
            {
                return;
            }
            foreach (var reg in registers.ToArray())
            {
                try
                {
                    reg.LateConfigRegist(DependencyHelper.ServiceCollection, logger);
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】后期依赖注册成功");
                }
                catch (Exception ex)
                {
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】后期依赖注册异常.{ex.Message}");
                    logger.Exception(ex);
                }
            }
        }
    }
}