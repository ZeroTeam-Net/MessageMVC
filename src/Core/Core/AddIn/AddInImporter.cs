using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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

        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Import { get; set; }

        /// <summary>
        /// 所有自动注册对象
        /// </summary>
        List<IAutoRegister> registers = new();

        /// <summary>
        /// 载入插件
        /// </summary>
        internal void LoadAddIn(ILogger logger)
        {
            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                ZeroAppOption.Instance.AddInPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else if (ZeroAppOption.Instance.AddInPath[0] != '/')
            {
                ZeroAppOption.Instance.AddInPath = Path.Combine(ZeroAppOption.Instance.RootPath, ZeroAppOption.Instance.AddInPath);
            }
            if (!Directory.Exists(ZeroAppOption.Instance.AddInPath))
            {
                logger.Information($"插件地址无效:{ZeroAppOption.Instance.AddInPath}");
                return;
            }
            logger.Information($"载入插件:{ZeroAppOption.Instance.AddInPath}");

            var files = Directory.GetFiles(ZeroAppOption.Instance.AddInPath, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (!CheckAssembly(file))
                    continue;
                // 通过容器对象将宿主和部件组装到一起。 
                try
                {
                    using var provider = new DirectoryCatalog(ZeroAppOption.Instance.AddInPath, Path.GetFileName(file));
                    using var c = new CompositionContainer(provider);
                    c.ComposeParts(this);
                    if (Import != null && Import.Any())
                    {
                        registers.AddRange(Import);
                    }
                    Import = null;
                }
                catch (System.Exception e2)
                {
                    logger.Exception(e2);
                }
            }
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