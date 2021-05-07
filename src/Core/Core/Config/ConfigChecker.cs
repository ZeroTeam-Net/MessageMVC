using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 配置的全局对象
    /// </summary>
    internal class ConfigChecker
    {
        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public void CheckLast(ZeroAppOption config, ILogger logger)
        {

            config.CopyByHase(ConfigurationHelper.Get<ZeroAppConfig>("MessageMVC:Option"));

            config.Services.Merge(config.ServiceMaps);

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
            ConfigurationHelper.RegistOnChange<ZeroAppConfig>("MessageMVC:Option", opt =>
            {
                if (opt != null)
                {
                    ZeroAppOption.Instance.IsOpenAccess = opt.IsOpenAccess;
                }
            });
            ConfigurationHelper.RegistOnChange<Dictionary<string, MessageTraceType>>("MessageMVC:MessageTrace", dict =>
            {
                if (dict != null)
                {
                    foreach (var kv in dict)
                    {
                        if (kv.Key == "Default")
                            ZeroAppOption.Instance.DefaultTraceOption = kv.Value;
                        else
                            ZeroAppOption.Instance.TraceOption[kv.Key] = kv.Value;
                    }
                }
            });
            ConfigurationHelper.RegistOnChange<Dictionary<string, string>>("MessageMVC:ServiceReplaceMap", dict =>
            {
                if (dict != null)
                {
                    foreach (var kv in dict)
                    {
                        ZeroAppOption.Instance.ServiceReplaceMap[kv.Key] = kv.Value;
                    }
                }
            });

            DependencyHelper.AddSingleton(config);

            DependencyHelper.Flush();
            //显示
            logger.Information($@"【当前配置信息】
            OS : {(ZeroAppOption.Instance.IsLinux ? "Linux" : "Windows")}
          Host : {ZeroAppOption.Instance.LocalIpAddress}
       BinPath : {ZeroAppOption.Instance.BinPath}
       AppName : {ZeroAppOption.Instance.AppName}
       Version : {ZeroAppOption.Instance.AppVersion}
      RunModel : {(ZeroAppOption.Instance.IsDevelopment ? "Development" : "Production")}
      LocalApp : {ZeroAppOption.Instance.LocalApp}
  LocalMachine : {ZeroAppOption.Instance.LocalMachine}
      RootPath : {ZeroAppOption.Instance.RootPath}
    DataFolder : {ZeroAppOption.Instance.DataFolder}
     AddInPath : {ZeroAppOption.Instance.AddInPath}
  ConfigFolder : {ZeroAppOption.Instance.ConfigFolder}
    ThreadPool : {ZeroAppOption.Instance.MaxWorkThreads:N0}worker|{ ZeroAppOption.Instance.MaxIOThreads:N0}threads
ApiServiceName : {ZeroAppOption.Instance.ApiServiceName}
    ServiceMap : {ZeroAppOption.Instance.ServiceReplaceMap.LinkToString(p => $"{p.Key}:{p.Value}")}
  MessageTrace : {ZeroAppOption.Instance.TraceOption.LinkToString(p => $"{p.Key}:{p.Value}")}");

        }

        /// <summary>
        ///     配置校验
        /// </summary>
        public void CheckBaseConfig()
        {
            var asName = Assembly.GetEntryAssembly().GetName();
            ZeroAppOption config = ZeroAppOption.Instance = new ZeroAppOption
            {
                AppName = asName.Name,
                AppVersion = asName.Version?.ToString(),
                BinPath = AppDomain.CurrentDomain.BaseDirectory,
                MaxCloseSecond = 30,
                DefaultTraceOption = MessageTraceType.Simple,
                ServiceReplaceMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                TraceOption = new Dictionary<string, MessageTraceType>(StringComparer.OrdinalIgnoreCase),
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                Services = new Messages.ServiceMap()
            };

            config.CopyByHase(ConfigurationHelper.Get<ZeroAppConfig>("MessageMVC:Option"));


            config.IsDevelopment = ConfigurationHelper.RunEnvironment.IsMe("Development");

            if (config.IsolateFolder)
            {
                config.RootPath = Path.GetDirectoryName(Environment.CurrentDirectory);
            }
            else
            {
                config.RootPath = Environment.CurrentDirectory;
            }
            if (config.AddInPath.IsPresent())
            {
                if (config.IsLinux && config.AddInPath[0] != '/')
                {
                    config.AddInPath = Path.Combine(config.RootPath, config.AddInPath);
                }
                if (!config.IsLinux && config.AddInPath[1] != ':')
                {
                    config.AddInPath = Path.Combine(config.RootPath, config.AddInPath);
                }
            }

            #region ServiceName

            try
            {
                config.HostName = Dns.GetHostName();
            }
            catch (Exception e)
            {
                Console.WriteLine($"【基础配置】\n{e}");
                config.HostName = config.HostName;
            }

            try
            {
                config.LocalIpAddress = GetHostIps();
            }
            catch (Exception e)
            {
                Console.WriteLine($"【基础配置】\n{e}");
            }

            config.LocalApp = config.AppVersion.IsMissing()
                ? (config.ShortName ?? config.AppName)
                : $"{config.ShortName ?? config.AppName}({config.AppVersion})";

            config.LocalMachine = config.LocalIpAddress.IsMissing()
                ? config.HostName
                : config.HostName.IsMissing()
                    ? config.LocalIpAddress
                    : $"{config.HostName}({config.LocalIpAddress})";

            #endregion

            #region Folder

            if (string.IsNullOrWhiteSpace(config.DataFolder))
            {
                config.DataFolder = IOHelper.CheckPath(config.RootPath, "datas");
            }

            if (string.IsNullOrWhiteSpace(config.ConfigFolder))
            {
                config.ConfigFolder = IOHelper.CheckPath(config.RootPath, "config");
            }
            var dynamicFileName = Path.Combine(config.ConfigFolder, "appsettings.dynamic.json");
            if (!File.Exists(dynamicFileName))
            {
                File.WriteAllText(dynamicFileName, "{}");
            }
            ConfigurationHelper.IncludeFile(dynamicFileName);
            IOHelper.Search(config.AddInPath, "*.json", false, (path, file) =>
            {
                if ("appsettings.json".IsMe(Path.GetFileName(file)))
                    ConfigurationHelper.IncludeFile(file);
            });
            IOHelper.Search(config.ConfigFolder, "*.json", false, (path, file) =>
            {
                if ("appsettings.json".IsMe(Path.GetFileName(file)))
                    ConfigurationHelper.IncludeFile(file);
            });

            #endregion

        }

        private static string GetHostIps()
        {
            var ips = new StringBuilder();
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
            return ips.ToString();
        }

    }
}
