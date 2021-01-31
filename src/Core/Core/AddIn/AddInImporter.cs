using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

namespace ZeroTeam.MessageMVC.AddIn
{
    /// <summary>
    /// MEF插件导入器
    /// </summary>
    public class AddInImporter
    {
        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Import { get; set; }

        /// <summary>
        /// 载入插件
        /// </summary>
        internal void LoadAddIn(ZeroAppOption config)
        {
            var logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(AddInImporter));
            logger.Information("载入插件");

            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                ZeroAppOption.Instance.AddInPath = Path.GetDirectoryName(GetType().Assembly.Location);

            }
            else if (ZeroAppOption.Instance.AddInPath[0] != '/')
            {
                ZeroAppOption.Instance.AddInPath = Path.Combine(ZeroAppOption.Instance.RootPath, ZeroAppOption.Instance.AddInPath);
            }
            if (!Directory.Exists(ZeroAppOption.Instance.AddInPath))
                return;

            var registers = new List<IAutoRegister>();

            var files = Directory.GetFiles(ZeroAppOption.Instance.AddInPath, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var first = name.Split('.', 2)[0];
                switch (first)
                {
                    case "System":
                    case "Microsoft":
                    case "NuGet":
                    case "Newtonsoft":
                        break;
                    default:
                        // 通过容器对象将宿主和部件组装到一起。 
                        try
                        {
                            using var provider = new DirectoryCatalog(ZeroAppOption.Instance.AddInPath, name);
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
                        break;
                }
            }

            if (registers.Count == 0)
            {
                return;
            }
            foreach (var reg in registers.ToArray())
            {
                try
                {
                    reg.AutoRegist(DependencyHelper.ServiceCollection, logger);
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】注册成功");
                }
                catch (System.Exception ex)
                {
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】注册异常.{ex.Message}");
                    logger.Exception(ex);
                }
            }
        }
    }
}