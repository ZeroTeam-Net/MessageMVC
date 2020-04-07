using Agebull.Common;
using Agebull.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.RedisMQ;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    ///   组件注册
    /// </summary>
    [Export(typeof(IAutoRegister))]
    [ExportMetadata("Symbol", '%')]
    public sealed class AutoRegister : IAutoRegister
    {
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist(IServiceCollection service)
        {
            RedisHelper.Initialization(new CSRedis.CSRedisClient(RedisOption.Instance.ConnectionString));

            service.UseCsRedis();
            ZeroFlowControl.Discove(this.GetType().Assembly);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        async void IAutoRegister.Initialize()
        {
            #region 配置分区与动态更新
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.ConfigFolder, "sync");
            foreach (var section in ConfigurationManager.Root.GetChildren())
            {
                if (section.Key == "ASPNETCORE_ENVIRONMENT_")
                    continue;
                if (section.Key != "MessageMVC")
                {
                    SaveOption(path, section);
                    continue;
                }
                foreach (var option in section.GetChildren())
                {
                    SaveOption(path, option);
                }
            }

            #endregion

            var sections = RedisHelper.HGetAll(ConfigChangOption.ConfigRedisKey);
            foreach (var section in sections)
            {
                await ConfigHelper.SaveToFile(section.Key, section.Value);
            }

        }

        private static void SaveOption(string path, IConfigurationSection option)
        {
            var file = Path.Combine(path, $"{option.Path.Replace(':', '.')}.json");
            if (!File.Exists(file))
                File.WriteAllText(file, "{}", Encoding.UTF8);

            ConfigurationManager.Builder.AddJsonFile(file, true, true);
        }
    }
}
