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
        ///   线程池最大工作线程数
        /// </summary>
        [DataMember] 
        public int MaxWorkThreads { get; set; }

        /// <summary>
        ///   线程池最大IO线程数
        /// </summary>
        [DataMember] 
        public int MaxIOThreads { get; set; }

        /// <summary>
        ///     站点数据使用AppName为文件夹
        /// </summary>
        [DataMember]
        public bool? StationIsolate { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }

        /// <summary>
        ///     短名称
        /// </summary>
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        ///     站点名称，注意唯一性
        /// </summary>
        [DataMember]
        public string StationName { get; set; }

        /// <summary>
        ///     本地数据文件夹
        /// </summary>
        [DataMember]
        public string DataFolder { get; set; }

        /// <summary>
        ///     本地日志文件夹
        /// </summary>
        [DataMember]
        public string LogFolder { get; set; }

        /// <summary>
        ///     本地配置文件夹
        /// </summary>
        [DataMember]
        public string ConfigFolder { get; set; }

        /// <summary>
        ///     应用所在的顶级目录
        /// </summary>
        [DataMember]
        public string RootPath { get; set; }

        /// <summary>
        ///     插件地址,如为空则与运行目录相同
        /// </summary>
        [DataMember]
        public string AddInPath { get; set; }

        /// <summary>
        /// 如果目标配置存在,则复制之
        /// </summary>
        /// <param name="option"></param>
        internal void CopyByHase(ZeroAppConfig option)
        {
            if (!string.IsNullOrWhiteSpace(option.AddInPath))
                AddInPath = option.AddInPath;
            if (!string.IsNullOrWhiteSpace(option.AddInPath))
                ConfigFolder = option.ConfigFolder;
            if (!string.IsNullOrWhiteSpace(option.LogFolder))
                LogFolder = option.LogFolder;
            if (!string.IsNullOrWhiteSpace(option.DataFolder))
                DataFolder = option.DataFolder;
            if (!string.IsNullOrWhiteSpace(option.StationName))
                StationName = option.StationName;
            if (!string.IsNullOrWhiteSpace(option.ShortName))
                ShortName = option.ShortName;
            if (!string.IsNullOrWhiteSpace(option.ServiceName))
                ServiceName = option.ServiceName;
            if (option.StationIsolate != null)
                StationIsolate = option.StationIsolate;
        }

        /// <summary>
        /// 如果本配置内容为空则用目标配置补全
        /// </summary>
        /// <param name="option"></param>
        public void CopyByEmpty(ZeroAppConfig option)
        {
            if (string.IsNullOrWhiteSpace(AddInPath))
                AddInPath = option.AddInPath;
            if (string.IsNullOrWhiteSpace(ConfigFolder))
                ConfigFolder = option.ConfigFolder;
            if (string.IsNullOrWhiteSpace(LogFolder))
                LogFolder = option.LogFolder;
            if (string.IsNullOrWhiteSpace(DataFolder))
                DataFolder = option.DataFolder;
            if (string.IsNullOrWhiteSpace(StationName))
                StationName = option.StationName;
            if (string.IsNullOrWhiteSpace(ShortName))
                ShortName = option.ShortName;
            if (string.IsNullOrWhiteSpace(ServiceName))
                ServiceName = option.ServiceName;
            if (StationIsolate == null)
                StationIsolate = option.StationIsolate;

        }
    }
}