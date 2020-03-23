using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
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
        string IFlowMiddleware.RealName => "ZeroGlobal";

        /// <summary>
        /// 等级
        /// </summary>
        int IFlowMiddleware.Level => int.MinValue;

        /// <summary>
        ///     关闭
        /// </summary>
        void IFlowMiddleware.Close()
        {
        }

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void IFlowMiddleware.CheckOption(ZeroAppConfigRuntime config)
        {
            CheckConfig(config);
            //上下文
            var testContext = IocHelper.Create<GlobalContext>();
            if (testContext == null)
                IocHelper.AddScoped<GlobalContext, GlobalContext>();
            GlobalContext.ServiceName = config.ServiceName;
            GlobalContext.ServiceRealName = $"{config.AppName}:{config.LocalIpAddress}:{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            //日志
            LogRecorder.LogPath = config.LogFolder;
            LogRecorder.GetMachineNameFunc = () => GlobalContext.ServiceRealName;
            LogRecorder.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.UserId.ToString() ?? "*";
            LogRecorder.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Request?.RequestId ?? RandomOperate.Generate(10);

            //显示
            ThreadPool.GetMaxThreads(out var worker, out var io);
            LogRecorder.SystemLog($@"
  RunModel : {ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"]}
        OS : {(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows")}
   AppName : {config.StationName} {config.AppName}
      Host : {GlobalContext.ServiceName} {config.LocalIpAddress}
  RealName : {GlobalContext.ServiceRealName}
  RootPath : {config.RootPath}
   LogPath : {LogRecorder.LogPath}
ThreadPool : {worker:N0}worker|{ io:N0}threads");
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        void CheckConfig(ZeroAppConfigRuntime config)
        {
            #region 配置组合

            var name = ConfigurationManager.Root["AppName"];
            if (name != null)
                config.AppName = name;

            if (string.IsNullOrWhiteSpace(config.AppName))
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
                file = Path.Combine(rootPath, "config", $"{config.AppName}.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
            }

            ConfigurationManager.BasePath 
                = ConfigurationManager.Root["rootPath"] 
                = config.RootPath 
                = rootPath;

            var cfg = ConfigurationManager.Get<ZeroAppConfig>(config.AppName);
            if (cfg != null)
                config.CopyByEmpty(cfg);
            #endregion

            #region ServiceName

            try
            {
                config.ServiceName = Dns.GetHostName();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                config.ServiceName = config.StationName;
            }
            config.LocalIpAddress = GetHostIps();

            config.ShortName = string.IsNullOrWhiteSpace(config.ShortName)
                ? config.StationName
                : config.ShortName.Trim();

            #endregion

            #region Folder

            if (config.StationIsolate == true)
            {
                if (string.IsNullOrWhiteSpace(config.DataFolder))
                    config.DataFolder = IOHelper.CheckPath(rootPath, "datas", config.AppName);

                if (string.IsNullOrWhiteSpace(config.LogFolder))
                    config.LogFolder = IOHelper.CheckPath(rootPath, "logs", config.AppName);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(config.DataFolder))
                    config.DataFolder = IOHelper.CheckPath(rootPath, "datas");

                if (string.IsNullOrWhiteSpace(config.LogFolder))
                    config.LogFolder = IOHelper.CheckPath(rootPath, "logs");

            }
            if (string.IsNullOrWhiteSpace(config.ConfigFolder))
                config.ConfigFolder = IOHelper.CheckPath(rootPath, "config");
            #endregion

        }

        string GetHostIps()
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
                        continue;
                    var ip = address.ToString();
                    if (ip == "127.0.0.1" || ip == "127.0.1.1" || ip == "::1" || ip == "-1")
                        continue;
                    if (first)
                        first = false;
                    else
                        ips.Append('|');
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
