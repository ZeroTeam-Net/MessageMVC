using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;

using Agebull.Common.Context;
using System.Threading;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     站点应用
    /// </summary>
    partial class ZeroApplication
    {
        /// <summary>
        ///     当前应用名称
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        ///     站点配置
        /// </summary>
        public static ZeroAppConfigRuntime Config { get; set; }

        /// <summary>
        ///     配置校验
        /// </summary>
        private static void CheckConfig()
        {
            #region 配置组合

            var name = ConfigurationManager.Root["AppName"];
            if (name != null)
                AppName = name;
            if (string.IsNullOrWhiteSpace(AppName))
                throw new Exception("无法找到配置[AppName],请在appsettings.json或代码中设置");

            var curPath = Environment.CurrentDirectory;
            string rootPath;

            if (ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"] == "Development")
            {
                rootPath = curPath;
            }
            else
            {
                rootPath = Path.GetDirectoryName(curPath);
                // ReSharper disable once AssignNullToNotNullAttribute
                var file = Path.Combine(rootPath, "config", "zero.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
                file = Path.Combine(rootPath, "config", $"{AppName}.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
            }
            ConfigurationManager.BasePath = ConfigurationManager.Root["rootPath"] = rootPath;

            var sec = ConfigurationManager.Get("Zero");
            if (sec == null)
                throw new Exception("无法找到主配置节点,路径为Zero,在zero.json或appsettings.json中设置");

            Config = new ZeroAppConfigRuntime
            {
                BinPath = curPath,
                RootPath = rootPath,
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            };
            var cfg = sec.Child<ZeroAppConfig>(AppName);
            if (cfg != null)
                Config.CopyByEmpty(cfg);
            cfg = sec.Child<ZeroAppConfig>("default");
            if (cfg != null)
                Config.CopyByEmpty(cfg);
            if (string.IsNullOrWhiteSpace(Config.StationName))
                Config.StationName = AppName;

            var glc = sec.Child<ZeroAppConfig>("Global");
            if (glc != null)
                Config.CopyByEmpty(glc);

            #endregion

            #region ServiceName

            if (string.IsNullOrWhiteSpace(Config.LocalIpAddress))
                Config.LocalIpAddress = GetHostIps();

            Config.ShortName = string.IsNullOrWhiteSpace(Config.ShortName)
                ? Config.StationName
                : Config.ShortName.Trim();

            if (string.IsNullOrWhiteSpace(Config.ServiceName))
            {
                try
                {
                    Config.ServiceName = Dns.GetHostName();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    Config.ServiceName = Config.StationName;
                }
            }

            #endregion

            #region Folder

            if (Config.StationIsolate == true)
            {
                if (string.IsNullOrWhiteSpace(Config.DataFolder))
                    Config.DataFolder = IOHelper.CheckPath(rootPath, "datas", AppName);

                if (string.IsNullOrWhiteSpace(Config.LogFolder))
                    Config.LogFolder = IOHelper.CheckPath(rootPath, "logs", AppName);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Config.DataFolder))
                    Config.DataFolder = IOHelper.CheckPath(rootPath, "datas");

                if (string.IsNullOrWhiteSpace(Config.LogFolder))
                    Config.LogFolder = IOHelper.CheckPath(rootPath, "logs");

            }
            if (string.IsNullOrWhiteSpace(Config.ConfigFolder))
                Config.ConfigFolder = IOHelper.CheckPath(rootPath, "config");
            #endregion

        }

        private static string GetHostIps()
        {
            var ips = new StringBuilder();
            try
            {
                var first = true;
                string hostName = Dns.GetHostName();
                LogRecorder.Trace("HostName:{0}", hostName);
                foreach (var address in Dns.GetHostAddresses(hostName))
                {
                    if (address.IsIPv4MappedToIPv6 || address.IsIPv6LinkLocal || address.IsIPv6Multicast ||
                        address.IsIPv6SiteLocal || address.IsIPv6Teredo)
                        continue;
                    var ip = address.ToString();
                    if (ip == "127.0.0.1" || ip == "127.0.1.1" || ip == "::1" || ip == "-1")
                        continue;
                    if (first)
                        first = false;
                    else
                        ips.Append(" , ");
                    ips.Append(ip);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }

            return ips.ToString();
        }

        private static void ShowOptionInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine($"  OS : {(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows")}");
            info.AppendLine($"  RunModel : {ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"]}");
            info.AppendLine($"  Name : {Config.StationName} {GlobalContext.ServiceName} {GlobalContext.ServiceRealName} {Config.LocalIpAddress}");

            info.AppendLine($"  RootPath : {Config.RootPath}");
            info.AppendLine($"  LogPath : {LogRecorder.LogPath}");

            ThreadPool.GetMaxThreads(out var worker, out var io);
            info.AppendLine($"  Worker threads: {worker:N0}Asynchronous I / O threads: { io:N0}");
            LogRecorder.SystemLog(info.ToString());
        }
    }
}