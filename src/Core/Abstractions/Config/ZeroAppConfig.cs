using System;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     本地站点配置
    /// </summary>
    [Serializable]
    [DataContract]
    public class ZeroAppConfig
    {
        /// <summary>
        ///     当前应用名称
        /// </summary>

        public string AppName { get; set; }

        /// <summary>
        ///     开放式访问
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
        ///     站点数据使用AppName为文件夹
        /// </summary>

        public bool IsolateFolder { get; set; }

        /// <summary>
        ///     本地数据文件夹
        /// </summary>

        public string DataFolder { get; set; }

        /// <summary>
        ///     本地配置文件夹
        /// </summary>

        public string ConfigFolder { get; set; }

        /// <summary>
        ///     插件地址,如为空则与运行目录相同
        /// </summary>

        public string AddInPath { get; set; }

        /// <summary>
        ///     启用插件自动加载
        /// </summary>

        public bool EnableAddIn { get; set; }

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

            if (option.IsolateFolder)
            {
                IsolateFolder = option.IsolateFolder;
            }

            if (!string.IsNullOrWhiteSpace(option.ConfigFolder))
            {
                ConfigFolder = option.ConfigFolder;
            }

            if (!string.IsNullOrWhiteSpace(option.DataFolder))
            {
                DataFolder = option.DataFolder;
            }

            if (option.EnableAddIn)
            {
                EnableAddIn = option.EnableAddIn;
            }

            if (!string.IsNullOrWhiteSpace(option.AddInPath))
            {
                AddInPath = option.AddInPath;
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

            if (string.IsNullOrWhiteSpace(AppName))
            {
                AppName = option.AppName;
            }
            if (IsolateFolder)
            {
                IsolateFolder = option.IsolateFolder;
            }

            if (string.IsNullOrWhiteSpace(ConfigFolder))
            {
                ConfigFolder = option.ConfigFolder;
            }

            if (string.IsNullOrWhiteSpace(DataFolder))
            {
                DataFolder = option.DataFolder;
            }

            if (EnableAddIn)
            {
                EnableAddIn = option.EnableAddIn;
            }

            if (string.IsNullOrWhiteSpace(AddInPath))
            {
                AddInPath = option.AddInPath;
            }
        }
        #endregion
    }
}