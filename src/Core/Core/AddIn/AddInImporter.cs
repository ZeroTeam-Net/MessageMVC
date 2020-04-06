using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

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
        int IZeroMiddleware.Level => short.MinValue;

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
            string path;
            if (string.IsNullOrEmpty(ZeroAppOption.Instance.AddInPath))
            {
                ZeroAppOption.Instance.AddInPath = path = Path.GetDirectoryName(this.GetType().Assembly.Location);
            }
            else
            {
                path = ZeroAppOption.Instance.AddInPath[0] == '/'
                     ? ZeroAppOption.Instance.AddInPath
                     : IOHelper.CheckPath(ZeroAppOption.Instance.RootPath, ZeroAppOption.Instance.AddInPath);
            }


            // 通过容器对象将宿主和部件组装到一起。 
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(directoryCatalog);
            container.ComposeParts(this);
            var logger = IocHelper.LoggerFactory.CreateLogger(nameof(AddInImporter));
            if (Registers == null)
            {
                return;
            }
            foreach (var reg in Registers)
            {
                logger.Information(() => reg.GetType().Assembly.FullName);
                reg.AutoRegist(IocHelper.ServiceCollection);
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