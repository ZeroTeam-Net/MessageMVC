using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
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
            var cfg = ConfigurationManager.Get("Redis").GetStr("ConnectionString");
            RedisHelper.Initialization(new CSRedis.CSRedisClient(cfg));

            service.UseCsRedis();
            ZeroFlowControl.Discove(this.GetType().Assembly);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void IAutoRegister.Initialize()
        {
            #region 配置分区与动态更新
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.ConfigFolder, "sync");
            foreach (var child in ConfigurationManager.Root.GetChildren())
            {
                if (child.Key == "ASPNETCORE_ENVIRONMENT_")
                    continue;
                var file = Path.Combine(path, $"{child.Key}.json");
                if (!File.Exists(file))
                    File.WriteAllText(file, "{}", Encoding.UTF8);
                ConfigurationManager.Builder.AddJsonFile(file, true, true);
            }
            #endregion

            var configs = RedisHelper.HGetAll(ConfigChangOption.ConfigRedisKey);
            foreach (var cfg in configs)
            {
                var file = Path.Combine(path, $"{cfg.Key}.json");
                //写入文件,更新留给Core自己处理
                if (File.Exists(file))
                    File.WriteAllText(file, cfg.Value ?? "{}", Encoding.UTF8);
            }
        }
    }
}
