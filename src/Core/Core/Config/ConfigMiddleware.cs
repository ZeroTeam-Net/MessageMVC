using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ZeroTeam.MessageMVC.Context;


namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 配置的全局对象
    /// </summary>
    public class ConfigMiddleware : IFlowMiddleware
    {
        /// <summary>
        /// 实例名称
        /// </summary>
        string IZeroMiddleware.Name => nameof(ConfigMiddleware);

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => int.MinValue;

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppOption config)
        {
            CheckConfig(config);
            IocHelper.AddSingleton(config);
            IocHelper.Update();
            LogRecorder.GetMachineNameFunc = () => config.TraceName;
            if (LogRecorder.UseBaseLogger)
            {
                var opt = ConfigurationManager.Get<TextLoggerOption>("Logging.Text");
                if (string.IsNullOrWhiteSpace(opt.path))
                {
                    LogRecorder.LogPath = config.IsolateFolder
                         ? IOHelper.CheckPath(config.RootPath, "logs", config.AppName)
                         : IOHelper.CheckPath(config.RootPath, "logs");
                }
            }
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
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        private void CheckConfig(ZeroAppOption config)
        {
            #region 配置组合

            bool useZero = !string.IsNullOrEmpty(config.RootPath);
            if (!useZero)
            {
                bool.TryParse(ConfigurationManager.Root["UseZero"], out useZero);
                if (!useZero || ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"] == "Development")
                {
                    config.RootPath = Environment.CurrentDirectory;
                }
                else
                {
                    config.RootPath = Path.GetDirectoryName(Environment.CurrentDirectory);
                }
            }
            ConfigurationManager.BasePath = config.RootPath;
            if (useZero)
            {
                var file = Path.Combine(config.RootPath, "config", "zero.json");
                if (File.Exists(file))
                {
                    ConfigurationManager.Load(file);
                    config.CopyByEmpty(ConfigurationManager.Get<ZeroAppConfig>("ZeroApp"));
                }
                file = Path.Combine(config.RootPath, "config", $"{config.AppName}.json");
                if (File.Exists(file))
                {
                    ConfigurationManager.Load(file);
                    config.CopyByHase(ConfigurationManager.Get<ZeroAppConfig>("ZeroApp"));
                }
            }
            #endregion

            #region ServiceName

            try
            {
                config.ServiceName = Dns.GetHostName();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
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
                LogRecorder.Exception(e);
            }

            return ips.ToString();
        }

    }
}
