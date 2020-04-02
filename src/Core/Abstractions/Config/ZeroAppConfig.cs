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
        public bool StationIsolate { get; set; }

        /// <summary>
        ///     短名称
        /// </summary>
        [DataMember]
        public string ShortName { get; set; }

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
        ///     插件地址,如为空则与运行目录相同
        /// </summary>
        [DataMember]
        public string AddInPath { get; set; }

        /// <summary>
        ///     启用插件自动加载
        /// </summary>
        [DataMember]
        public bool EnableAddIn { get; set; }

        /// <summary>
        ///     启用全局上下文
        /// </summary>
        [DataMember]
        public bool EnableGlobalContext { get; set; }

        /// <summary>
        ///     启用日志记录器LogRecorder
        /// </summary>
        [DataMember]
        public bool EnableLogRecorder { get; set; }

        /// <summary>
        ///     启用埋点
        /// </summary>
        [DataMember]
        public bool EnableMarkPoint { get; set; }

        /// <summary>
        ///     埋点服务名称
        /// </summary>
        [DataMember]
        public string MarkPointName { get; set; }

        /// <summary>
        ///     启用异常消息本地重放
        /// </summary>
        [DataMember]
        public bool EnableMessageReConsumer { get; set; }


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

            if (option.StationIsolate)
            {
                StationIsolate = option.StationIsolate;
            }

            if (!string.IsNullOrWhiteSpace(option.ConfigFolder))
            {
                ConfigFolder = option.ConfigFolder;
            }

            if (!string.IsNullOrWhiteSpace(option.DataFolder))
            {
                DataFolder = option.DataFolder;
            }

            if (!string.IsNullOrWhiteSpace(option.ShortName))
            {
                ShortName = option.ShortName;
            }

            if (option.EnableAddIn)
            {
                EnableGlobalContext = option.EnableAddIn;
            }

            if (!string.IsNullOrWhiteSpace(option.AddInPath))
            {
                AddInPath = option.AddInPath;
            }

            if (option.EnableGlobalContext)
            {
                EnableGlobalContext = option.EnableGlobalContext;
            }

            if (option.EnableLogRecorder)
            {
                EnableLogRecorder = option.EnableLogRecorder;
            }

            if (!string.IsNullOrWhiteSpace(option.LogFolder))
            {
                LogFolder = option.LogFolder;
            }

            if (option.EnableMessageReConsumer)
            {
                EnableMessageReConsumer = option.EnableMessageReConsumer;
            }

            if (option.EnableMarkPoint)
            {
                EnableMarkPoint = option.EnableMarkPoint;
            }

            if (!string.IsNullOrWhiteSpace(option.MarkPointName))
            {
                MarkPointName = option.MarkPointName;
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

            if (StationIsolate)
            {
                StationIsolate = option.StationIsolate;
            }

            if (string.IsNullOrWhiteSpace(ConfigFolder))
            {
                ConfigFolder = option.ConfigFolder;
            }

            if (string.IsNullOrWhiteSpace(DataFolder))
            {
                DataFolder = option.DataFolder;
            }

            if (string.IsNullOrWhiteSpace(ShortName))
            {
                ShortName = option.ShortName;
            }

            if (EnableAddIn)
            {
                EnableGlobalContext = option.EnableAddIn;
            }

            if (string.IsNullOrWhiteSpace(AddInPath))
            {
                AddInPath = option.AddInPath;
            }

            if (EnableGlobalContext)
            {
                EnableGlobalContext = option.EnableGlobalContext;
            }

            if (EnableLogRecorder)
            {
                EnableLogRecorder = option.EnableLogRecorder;
            }

            if (string.IsNullOrWhiteSpace(LogFolder))
            {
                LogFolder = option.LogFolder;
            }

            if (EnableMessageReConsumer)
            {
                EnableMessageReConsumer = option.EnableMessageReConsumer;
            }

            if (EnableMarkPoint)
            {
                EnableMarkPoint = option.EnableMarkPoint;
            }

            if (string.IsNullOrWhiteSpace(MarkPointName))
            {
                MarkPointName = option.MarkPointName;
            }
        }
    }
}