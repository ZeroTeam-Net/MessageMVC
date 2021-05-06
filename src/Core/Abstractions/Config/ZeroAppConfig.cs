using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     本地站点配置
    /// </summary>
    public class ZeroAppConfig
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
        /// 使用上级目录作为基础目录
        /// </summary>
        public bool IsolateFolder { get; set; }

        /// <summary>
        ///     当前应用简称
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        ///     开放式访问(可动态修改)
        /// </summary>
        public bool IsOpenAccess { get; set; }

        /// <summary>
        ///   线程池最大工作线程数
        /// </summary>
        public int MaxWorkThreads { get; set; }

        /// <summary>
        ///   线程池最大IO线程数
        /// </summary>
        public int MaxIOThreads { get; set; }

        /// <summary>
        ///   关闭最大等待时长
        /// </summary>
        public int MaxCloseSecond { get; set; }

        /// <summary>
        ///     插件地址,如为空则与运行目录相同
        /// </summary>
        public string AddInPath { get; set; }

        /// <summary>
        ///     使用System.Text.Json序列化，而不是使用默认的Newtonsoft.Json
        /// </summary>
        public bool UsMsJson { get; set; }

        /// <summary>
        ///     当前接口服务名称（未明显设置Service属性的Controler使用）
        /// </summary>
        public string ApiServiceName { get; set; }

        /// <summary>
        ///     默认使用的传输器
        /// </summary>
        public string DefaultPoster { get; set; }

        /// <summary>
        ///     启用本地隧道（即本地接收器存在的话，本地处理）
        /// </summary>
        public bool LocalTunnel { get; set; }

        /// <summary>
        /// 服务字典
        /// </summary>
        public Dictionary<string,string[]> ServiceMaps { get; set; }

        #region 复制

        /// <summary>
        /// 如果目标配置存在,则复制之
        /// </summary>
        /// <param name="option"></param>
        public void CopyByHase(ZeroAppConfig option)
        {
            if (option == null)
            {
                return;
            }

            if (option.AppName.IsPresent())
            {
                AppName = option.AppName;
            }
            if (option.DefaultPoster.IsPresent())
            {
                DefaultPoster = option.DefaultPoster;
            }
            if (option.LocalTunnel)
            {
                LocalTunnel = option.LocalTunnel;
            }
            if (option.ApiServiceName.IsPresent())
            {
                ApiServiceName = option.ApiServiceName;
            }
            if (option.ShortName.IsPresent())
            {
                ShortName = option.ShortName;
            }

            if (option.MaxIOThreads > 0)
            {
                MaxIOThreads = option.MaxIOThreads;
            }
            if (option.MaxWorkThreads > 0)
            {
                MaxWorkThreads = option.MaxWorkThreads;
            }
            if (option.MaxCloseSecond > 0)
            {
                MaxCloseSecond = option.MaxCloseSecond;
            }
            if (option.IsOpenAccess)
            {
                IsOpenAccess = option.IsOpenAccess;
            }

            if (option.AppVersion.IsPresent())
            {
                AppVersion = option.AppVersion;
            }

            if (option.IsolateFolder)
            {
                IsolateFolder = option.IsolateFolder;
            }
            if (option.AddInPath.IsPresent())
            {
                AddInPath = option.AddInPath;
            }
            if (option.UsMsJson)
            {
                UsMsJson = option.UsMsJson;
            }
            if (option.ServiceMaps != null)
            {
                ServiceMaps = option.ServiceMaps;
            }
            
        }

        /// <summary>
        /// 如果本配置内容为空则用目标配置补全
        /// </summary>
        /// <param name="option"></param>
        public void CopyByEmpty(ZeroAppConfig option)
        {
            if (option == null)
            {
                return;
            }

            if (ShortName.IsMissing())
            {
                ShortName = option.ShortName;
            }
            if (ApiServiceName.IsMissing())
            {
                ApiServiceName = option.ApiServiceName;
            }
            if (AppName.IsMissing())
            {
                AppName = option.AppName;
            }
            if (AppVersion.IsMissing())
            {
                AppVersion = option.AppVersion;
            }

            if (!IsolateFolder)
            {
                IsolateFolder = option.IsolateFolder;
            }
            if (!IsOpenAccess)
            {
                IsOpenAccess = option.IsOpenAccess;
            }
            if (MaxIOThreads <= 0)
            {
                MaxIOThreads = option.MaxIOThreads;
            }
            if (MaxWorkThreads <= 0)
            {
                MaxWorkThreads = option.MaxWorkThreads;
            }
            if (MaxCloseSecond <= 0)
            {
                MaxCloseSecond = option.MaxCloseSecond;
            }
            if (AddInPath.IsMissing())
            {
                AddInPath = option.AddInPath;
            }
            if (!UsMsJson)
                UsMsJson = option.UsMsJson;
        }

        #endregion
    }

}