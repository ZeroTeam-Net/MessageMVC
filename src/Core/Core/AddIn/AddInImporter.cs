using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

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
        /// 运行状态
        /// </summary>
        public StationStateType State { get; } = StationStateType.Run;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroMiddleware.Name => "AddInImporter";

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => -0xFFF;

        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Registers { get; set; }

        /// <summary>
        /// 配置校验
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppOption config)
        {
            DirectoryCatalog directoryCatalog;
            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                directoryCatalog = new DirectoryCatalog(Path.GetDirectoryName(this.GetType().Assembly.Location),
                    "ZeroTeam.MessageMVC.*.dll");
            }
            else
            {
                directoryCatalog = new DirectoryCatalog(ZeroAppOption.Instance.AddInPath[0] == '/'
                     ? ZeroAppOption.Instance.AddInPath
                     : IOHelper.CheckPath(ZeroAppOption.Instance.RootPath, ZeroAppOption.Instance.AddInPath));
            }

            var logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(AddInImporter));

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
                logger.Information(() => reg.GetType().Assembly.FullName);
                try
                {
                    reg.AutoRegist(DependencyHelper.ServiceCollection);
                }
                catch (System.Exception ex)
                {
                    logger.Exception(ex);
                }
            }

        }

        /// <summary>
        /// 初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            if (Registers == null)
            {
                return;
            }
            foreach (var reg in Registers)
            {
                reg.Initialize();
            }
        }
    }
}