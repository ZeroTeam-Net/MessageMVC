using System.Collections.Generic;

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
        ///     当前接口服务名称（未明显设置Service属性的Controler使用）
        /// </summary>
        public string ApiServiceName { get; set; }

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

            if (!string.IsNullOrWhiteSpace(option.AppName))
            {
                AppName = option.AppName;
            }
            if (!string.IsNullOrWhiteSpace(option.ApiServiceName))
            {
                ApiServiceName = option.ApiServiceName;
            }
            if (!string.IsNullOrWhiteSpace(option.ShortName))
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

            if (!string.IsNullOrWhiteSpace(option.AppVersion))
            {
                AppVersion = option.AppVersion;
            }

            if (option.IsolateFolder)
            {
                IsolateFolder = option.IsolateFolder;
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

            if (string.IsNullOrWhiteSpace(ShortName))
            {
                ShortName = option.ShortName;
            }
            if (!string.IsNullOrWhiteSpace(ApiServiceName))
            {
                ApiServiceName = option.ApiServiceName;
            }
            if (string.IsNullOrWhiteSpace(AppName))
            {
                AppName = option.AppName;
            }
            if (string.IsNullOrWhiteSpace(AppVersion))
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
        }

        #endregion
    }
}