using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.EntityModel.Common;
using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
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
                ZeroFlowControl.Discove(GetType().Assembly);

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

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.Start()
        {
            if (ConfigChangOption.Instance.IsService)
                return;
            _ = Start();
        }

        /// <summary>
        /// 注册
        /// </summary>
        async Task Start()
        {
            var services = ZeroFlowControl.Services.Values.Where(p => p.IsAutoService && p.Receiver.PosterName != null);
            List<NameValue> posters = new List<NameValue>();
            foreach (var service in services)
            {
                posters.Add(new NameValue(service.Receiver.PosterName, service.ServiceName));
            }
            if (posters.Count > 0)
                await MessagePoster.CallAsync("ConfigEdit", "v1/regist", posters);
            var url = ConfigurationManager.Root.GetSection("Kestrel:Endpoints:Http:Url")?.Value;
            if (url == null)
                return;
            var ip = ZeroAppOption.Instance.LocalIpAddress.Split('|')[0];
            url = url.Replace("0.0.0.0", ip)
                     .Replace("*", ip);
            var https = posters.Where(p => p.Name == "HttpPoster").Select(p => new HttpClientItem
            {
                Name = p.Value,
                Url = url,
                Services = p.Value
            }).ToList();
            if (https.Count > 0)
                await MessagePoster.CallAsync("ConfigEdit", "v1/http", https);
        }
    }
}
