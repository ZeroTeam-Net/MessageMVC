using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Agebull.Common;
using Agebull.Common.Ioc;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// MEF插件导入器
    /// </summary>
    public class AddInImporter : IAppMiddleware
    {
        /// <summary>
        /// 等级
        /// </summary>
        int IAppMiddleware.Level => -0xFFFF;

        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Registers { get; set; }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public void CheckOption(ZeroAppConfigRuntime config)
        {
            if (string.IsNullOrEmpty(ZeroApplication.Config.AddInPath))
                return;

            var path = ZeroApplication.Config.AddInPath[0] == '/'
                ? ZeroApplication.Config.AddInPath
                : IOHelper.CheckPath(ZeroApplication.Config.RootPath, ZeroApplication.Config.AddInPath);
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
                return;
            foreach (var reg in Registers)
                reg.AutoRegist();
            foreach (var reg in Registers)
            {
                reg.Initialize();
            }
        }
    }
}