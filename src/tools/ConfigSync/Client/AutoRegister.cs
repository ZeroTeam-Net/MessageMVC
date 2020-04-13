using Agebull.Common;
using Agebull.Common.Configuration;
using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Composition;
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
        async void IAutoRegister.AutoRegist(IServiceCollection services)
        {
            services.UseCsRedis();
            if (!ConfigChangOption.Instance.IsService)
                ZeroFlowControl.Discove(this.GetType().Assembly);

            #region 配置分区与动态更新
            var path = IOHelper.CheckPath(ZeroAppOption.Instance.ConfigFolder, "sync");
            foreach (var section in ConfigurationManager.Root.GetChildren())
            {
                if (section.Key == "ASPNETCORE_ENVIRONMENT_")
                    continue;
                if (section.Key != "MessageMVC")
                {
                    await ConfigHelper.SaveOption(path, section);
                    continue;
                }
                foreach (var option in section.GetChildren())
                {
                    await ConfigHelper.SaveOption(path, option);
                }
            }

            #endregion

            #region Redis读取

            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            var sections = await redis.HGetAllAsync(ConfigChangOption.ConfigRedisKey);
            foreach (var section in sections)
            {
                await ConfigHelper.SaveToFile(section.Key, section.Value);
            }
            #endregion
            ConfigurationManager.Flush();
        }
    }
}
