using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     本地站点配置
    /// </summary>
    public class ZeroAppOption : ZeroAppConfig
    {
        /// <summary>
        ///     当前应用名称
        /// </summary>
        
        public string AppName { get; set; }

        /// <summary>
        ///     当前应用版本号
        /// </summary>
        
        public string AppVersion { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        
        public string ServiceName { get; set; }

        /// <summary>
        ///     当前服务器的跟踪名称
        /// </summary>
        public string TraceName { get; set; }

        /// <summary>
        ///     应用所在的顶级目录
        /// </summary>
        
        public string RootPath { get; set; }

        /// <summary>
        ///     程序所在地址
        /// </summary>
        [IgnoreDataMember]
        public string BinPath { get; set; }

        /// <summary>
        ///     本机IP地址
        /// </summary>
        [IgnoreDataMember]
        public string LocalIpAddress { get; set; }

        /// <summary>
        ///     是否在Linux黑环境下
        /// </summary>
        [IgnoreDataMember]
        public bool IsLinux { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static ZeroAppOption Instance { get;}

        static ZeroAppOption()
        {
            var asName = Assembly.GetEntryAssembly().GetName();
            Instance = new ZeroAppOption
            {
                AppName = asName.Name,
                AppVersion = asName.Version?.ToString(),
                BinPath = Environment.CurrentDirectory,
                RootPath = Environment.CurrentDirectory,
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            };
            Instance.CopyByHase(ConfigurationManager.Get<ZeroAppConfig>("ZeroApp"));

        }
    }
}