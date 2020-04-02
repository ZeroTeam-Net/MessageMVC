using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
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
        string IZeroMiddleware.Name => "ZeroGlobal";

        /// <summary>
        /// 等级
        /// </summary>
        int IZeroMiddleware.Level => int.MinValue;

        /// <summary>
        ///     关闭
        /// </summary>
        void IFlowMiddleware.Close()
        {
        }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppOption config)
        {
            CheckConfig(config);
            IocHelper.Update();
            //上下文
            if (config.EnableGlobalContext)
            {
                GlobalContext.AppName = config.AppName;
                GlobalContext.ServiceName = config.ServiceName;
            }
            //日志
            if (config.EnableLogRecorder)
            {
                LogRecorder.LogPath = config.LogFolder;
                LogRecorder.GetMachineNameFunc = () => config.RealName;
                if (config.EnableGlobalContext)
                {
                    LogRecorder.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.UserId.ToString() ?? "*";
                    LogRecorder.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Request?.RequestId ?? RandomCode.Generate(10);
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
            //显示
            LogRecorder.SystemLog($@"
      AppName : {config.AppName}
      Version : {config.AppVersion}
  ServiceName : {config.ServiceName}
     RunModel : {ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"]}
           OS : {(config.IsLinux ? "Linux" : "Windows")}
         Host : {config.ServiceName} {config.LocalIpAddress}
     RealName : {config.RealName}
     RootPath : {config.RootPath}
    AddInPath : {config.AddInPath}({(config.EnableGlobalContext ? "Enable" : "Disable")})
      LogPath : {config.LogFolder}({(config.EnableLogRecorder ? "Enable" : "Disable")})
     DataPath : {config.DataFolder}
   ConfigPath : {config.ConfigFolder}
   ThreadPool : {config.MaxWorkThreads:N0}worker|{ config.MaxIOThreads:N0}threads
GlobalContext : {(config.EnableGlobalContext ? "Enable" : "Disable")}
   ReConsumer : {(config.EnableMessageReConsumer ? "Enable" : "Disable")}
    MarkPoint : {config.MarkPointName}({(config.EnableMarkPoint ? "Enable" : "Disable")})");
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        private void CheckConfig(ZeroAppOption config)
        {
            #region 配置组合

            config.CopyByHase(ConfigurationManager.Get<ZeroAppConfig>("ZeroApp"));
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

            config.ShortName = string.IsNullOrWhiteSpace(config.ShortName)
                ? config.ServiceName
                : config.ShortName.Trim();
            config.RealName = $"{config.AppName}v{config.AppVersion}({config.LocalIpAddress})";
            #endregion

            #region Folder

            if (config.StationIsolate == true)
            {
                if (string.IsNullOrWhiteSpace(config.DataFolder))
                {
                    config.DataFolder = IOHelper.CheckPath(config.RootPath, "datas", config.AppName);
                }

                if (string.IsNullOrWhiteSpace(config.LogFolder))
                {
                    config.LogFolder = IOHelper.CheckPath(config.RootPath, "logs", config.AppName);
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

                if (string.IsNullOrWhiteSpace(config.LogFolder))
                {
                    config.LogFolder = IOHelper.CheckPath(config.RootPath, "logs");
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
