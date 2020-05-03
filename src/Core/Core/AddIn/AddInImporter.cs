using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
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
        public IEnumerable<IAutoRegister> Registers { get; set; }

        /// <summary>
        /// 检查
        /// </summary>
        async Task ILifeFlow.Check(ZeroAppOption config)
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(AddInImporter));
            logger.Information("AddInImporter >>> 检查");
            DirectoryCatalog directoryCatalog;
            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                directoryCatalog = new DirectoryCatalog(Path.GetDirectoryName(GetType().Assembly.Location),"*.dll");
            }
            else
            {
                directoryCatalog = new DirectoryCatalog(ZeroAppOption.Instance.AddInPath[0] == '/'
                     ? ZeroAppOption.Instance.AddInPath
                     : IOHelper.CheckPath(ZeroAppOption.Instance.RootPath, ZeroAppOption.Instance.AddInPath));
            }


            // 通过容器对象将宿主和部件组装到一起。 
            try
            {
                new CompositionContainer(directoryCatalog).ComposeParts(this);
            }
            catch (System.Exception e2)
            {
                logger.Exception(e2);
            }
            if (Registers == null)
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