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
        ///     当前接口服务名称（未明显设置Service属性的Controler使用）
        /// </summary>
        public string ApiServiceName { get; set; }

        /// <summary>
        ///     当前应用简称
        /// </summary>
        public string ShortName { get; set; }

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

        /*// <summary>
        ///     启用插件自动加载
        /// </summary>
        public bool EnableAddIn { get; set; }*/

        /// <summary>
        ///     服务映射，即将对应服务名称替换成另一个服务
        /// </summary>
        public Dictionary<string, string> ServiceMap { get; set; }

        /// <summary>
        /// 跟踪信息内容
        /// </summary>
        public TraceInfoType TraceInfo { get; set; }


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
            TraceInfo |= option.TraceInfo;
            if (ServiceMap == null)
            {
                ServiceMap = option.ServiceMap;
            }
            else if (option.ServiceMap != null)
            {
                foreach (var kv in option.ServiceMap)
                    ServiceMap[kv.Key] = kv.Value;
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

            //if (option.EnableAddIn)
            //{
            //    EnableAddIn = option.EnableAddIn;
            //}
            if (option.IsOpenAccess)
            {
                IsOpenAccess = option.IsOpenAccess;
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
            TraceInfo |= option.TraceInfo;
            if (ServiceMap == null)
            {
                ServiceMap = option.ServiceMap;
            }
            else if (option.ServiceMap != null)
            {
                foreach (var kv in option.ServiceMap)
                    ServiceMap[kv.Key] = kv.Value;
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
            if (!IsolateFolder)
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

            //if (!EnableAddIn)
            //{
            //    EnableAddIn = option.EnableAddIn;
            //}
            if (!IsOpenAccess)
            {
                IsOpenAccess = option.IsOpenAccess;
            }

            if (string.IsNullOrWhiteSpace(AddInPath))
            {
                AddInPath = option.AddInPath;
            }
        }
        #endregion
    }
}