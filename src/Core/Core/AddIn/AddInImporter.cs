using Agebull.Common;
using Agebull.Common.Ioc;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// MEF插件导入器
    /// </summary>
    public class AddInImporter : IFlowMiddleware
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; } = StationStateType.Run;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IFlowMiddleware.RealName => "AddInImporter";

        /// <summary>
        /// 等级
        /// </summary>
        int IFlowMiddleware.Level => short.MinValue;

        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Registers { get; set; }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public void CheckOption(ZeroAppOption config)
        {
            if (string.IsNullOrEmpty(ZeroFlowControl.Config.AddInPath))
            {
                return;
            }

            var path = ZeroFlowControl.Config.AddInPath[0] == '/'
                ? ZeroFlowControl.Config.AddInPath
                : IOHelper.CheckPath(ZeroFlowControl.Config.RootPath, ZeroFlowControl.Config.AddInPath);
            ZeroTrace.SystemLog("AddIn(Service)", path);
            // 通过容器对象将宿主和部件组装到一起。 
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(directoryCatalog);
            container.ComposeParts(this);
            foreach (var reg in Registers)
            {
                ZeroTrace.SystemLog("AddIn(Extend)", reg.GetType().Assembly.FullName);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            if (Registers == null)
            {
                return;
            }

            foreach (var reg in Registers)
            {
                reg.AutoRegist();
            }

            foreach (var reg in Registers)
            {
                reg.Initialize();
            }
        }
    }
}