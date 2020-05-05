using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.AddIn
{
    /// <summary>
    /// MEF插件导入器
    /// </summary>
    public class AddInImporter : IFlowMiddleware
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static AddInImporter Instance = new AddInImporter();


        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(AddInImporter);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Framework;
        ILogger logger;
        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Import { get; set; }

        /// <summary>
        /// 插件对象
        /// </summary>
        List<IAutoRegister> Registers = new List<IAutoRegister>();

        /// <summary>
        /// 检查
        /// </summary>
        async Task ILifeFlow.Check(ZeroAppOption config)
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(AddInImporter));
            logger.Information("AddInImporter >>> 检查");

            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                //using var catalog = new AggregateCatalog();
                var path = Path.GetDirectoryName(GetType().Assembly.Location);
                var files = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
                foreach(var file in files)
                {
                    var name = Path.GetFileName(file);
                    var first = name.Split('.',2)[0];
                    switch(first)
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
                                using var provider = new DirectoryCatalog(path, name);
                                using var c = new CompositionContainer(provider);
                                c.ComposeParts(this);
                                if (Import != null && Import.Any())
                                {
                                    Registers.AddRange(Import);
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
                // 通过容器对象将宿主和部件组装到一起。 
                //try
                //{
                //    using var provider = new CompositionContainer(catalog);
                //    using var c = new CompositionContainer(provider);
                //    c.ComposeParts(this);
                //}
                //catch (System.Exception e2)
                //{
                //    logger.Exception(e2);
                //}
            }
            else
            {
                var path = ZeroAppOption.Instance.AddInPath[0] == '/'
                        ? ZeroAppOption.Instance.AddInPath
                        : Path.Combine(ZeroAppOption.Instance.RootPath, ZeroAppOption.Instance.AddInPath);
                if (!Directory.Exists(path))
                    return;
                // 通过容器对象将宿主和部件组装到一起。 
                try
                {
                    using var provider = new DirectoryCatalog(path, "*.dll");
                    using var c = new CompositionContainer(provider);
                    c.ComposeParts(this);
                }
                catch (System.Exception e2)
                {
                    logger.Exception(e2);
                }
            }


            if (Registers.Count == 0)
            {
                return;
            }
            foreach (var reg in Registers)
            {
                try
                {
                    await reg.AutoRegist(DependencyHelper.ServiceCollection);
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】注册成功");
                }
                catch (System.Exception ex)
                {
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】注册异常.{ex.Message}");
                    logger.Exception(ex);
                }
            }
            foreach (var reg in Registers)
            {
                try
                {
                    await reg.Check(config);
                }
                catch (System.Exception ex)
                {
                    logger.Information(() => $"插件【{reg.GetType().Assembly.FullName}】预检异常.{ex.Message}");
                    logger.Exception(ex);
                }
            }
        }

        /// <summary>
        /// 准备
        /// </summary>
        async Task ILifeFlow.Initialize()
        {
            logger.Information("AddInImporter >>> 准备");
            if (Registers == null)
            {
                return;
            }
            foreach (var reg in Registers)
            {
                await reg.Initialize();
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        Task ILifeFlow.Open()
        {
            logger.Information("AddInImporter >>> 启动");
            if (Registers == null)
            {
                return Task.CompletedTask;
            }
            foreach (var reg in Registers)
            {
                _ = reg.Open();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        async Task ILifeFlow.Close()
        {
            logger.Information("AddInImporter >>> 关闭");
            if (Registers == null)
            {
                return;
            }
            foreach (var reg in Registers)
            {
                await reg.Close();
            }
        }


        /// <summary>
        /// 注销
        /// </summary>
        async Task ILifeFlow.Destory()
        {
            logger.Information("AddInImporter >>> 注销");
            if (Registers == null)
            {
                return;
            }
            foreach (var reg in Registers)
            {
                await reg.Destory();
            }
        }
    }
}