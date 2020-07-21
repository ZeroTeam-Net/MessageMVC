using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 配置的全局对象
    /// </summary>
    public class ConfigMiddleware : IFlowMiddleware
    {
        /// <summary>
        /// 调用的内容
        /// </summary>
        internal ILogger logger;

        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroDependency.Name => nameof(ConfigMiddleware);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.Basic;

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        Task ILifeFlow.Check(ZeroAppOption config)
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger<ConfigMiddleware>();
            CheckConfig(config);
            DependencyHelper.AddSingleton(config);
            DependencyHelper.Update();

            //线程数
            if (config.MaxIOThreads > 0 && config.MaxWorkThreads > 0)
            {
                ThreadPool.SetMaxThreads(config.MaxWorkThreads, config.MaxIOThreads);
            }
            else
            {
                ThreadPool.GetMaxThreads(out var worker, out var io);
                config.MaxIOThreads = io;
                config.MaxWorkThreads = worker;
            }
            return Task.CompletedTask;
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        private void CheckConfig(ZeroAppOption config)
        {
            #region 配置组合

            bool useZero = !string.IsNullOrEmpty(config.RootPath);
            config.IsDevelopment = ConfigurationHelper.Root["ASPNETCORE_ENVIRONMENT_"] == "Development";
            if (!useZero)
            {
                bool.TryParse(ConfigurationHelper.Root["MessageMVC:Option:UseZero"] ?? "false", out useZero);
                if (!useZero || config.IsDevelopment)
                {
                    config.RootPath = Environment.CurrentDirectory;
                }
                else
                {
                    config.RootPath = Path.GetDirectoryName(Environment.CurrentDirectory);
                }
            }
            ConfigurationHelper.BasePath = config.RootPath;
            if (useZero)
            {
                var file = Path.Combine(config.RootPath, "config", "zero.json");
                if (File.Exists(file))
                {
                    ConfigurationHelper.Load(file);
                    config.CopyByEmpty(ConfigurationHelper.Get<ZeroAppConfig>("MessageMVC:Option"));
                }
                file = Path.Combine(config.RootPath, "config", $"{config.AppName}.json");
                if (File.Exists(file))
                {
                    ConfigurationHelper.Load(file);
                    config.CopyByHase(ConfigurationHelper.Get<ZeroAppConfig>("MessageMVC:Option"));
                }
            }

            config.ServiceMap ??= new Dictionary<string, string>();
            #endregion

            #region ServiceName

            try
            {
                config.ServiceName = Dns.GetHostName();
            }
            catch (Exception e)
            {
                logger.Exception(e);
                config.ServiceName = config.ServiceName;
            }
            config.LocalIpAddress = GetHostIps();

            config.TraceName = $"{config.AppName}({config.AppVersion})|{config.ServiceName}|{config.LocalIpAddress}";

            #endregion

            #region Folder

            if (config.IsolateFolder == true)
            {
                if (string.IsNullOrWhiteSpace(config.DataFolder))
                {
                    config.DataFolder = IOHelper.CheckPath(config.RootPath, "datas", config.AppName);
                }

                if (string.IsNullOrWhiteSpace(config.ConfigFolder))
                {
                    config.ConfigFolder = IOHelper.CheckPath(config.RootPath, "config", config.AppName);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(config.DataFolder))
                {
                    config.DataFolder = IOHelper.CheckPath(config.RootPath, "datas");
                }

                if (string.IsNullOrWhiteSpace(config.ConfigFolder))
                {
                    config.ConfigFolder = IOHelper.CheckPath(config.RootPath, "config");
                }
            }
            #endregion
        }

        private string GetHostIps()
        {
            var ips = new StringBuilder();
            try
            {
                var first = true;
                string hostName = Dns.GetHostName();
                foreach (var address in Dns.GetHostAddresses(hostName))
                {
                    if (address.IsIPv4MappedToIPv6 || address.IsIPv6LinkLocal || address.IsIPv6Multicast ||
                        address.IsIPv6SiteLocal || address.IsIPv6Teredo)
                    {
                        continue;
                    }

                    var ip = address.ToString();
                    if (ip == "127.0.0.1" || ip == "127.0.1.1" || ip == "::1" || ip == "-1")
                    {
                        continue;
                    }

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        ips.Append('|');
                    }

                    ips.Append(ip);
                }
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }

            return ips.ToString();
        }

    }
}
