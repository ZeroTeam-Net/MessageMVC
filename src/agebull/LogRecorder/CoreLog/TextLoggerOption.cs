// 所在工程：Agebull.EntityModel.Core
// 整理用户：agebull
// 建立时间：2020-03-03 12:01:20
// 整理时间：2020-03-03 12:01:20

#region

using Microsoft.Extensions.Logging;
using System;
using System.IO;
#endregion

namespace Agebull.Common.Logging
{
    /// <summary>
    /// 文本日志配置
    /// </summary>
    public class TextLoggerOption
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 最大文件数量
        /// </summary>
        public int MaxFile { get; set; }

        /// <summary>
        /// 最小可用空间(小于时只记录系统与错误日志)
        /// </summary>
        public int MinFreeSize { get; set; }

        /// <summary>
        /// 每日一个文件夹吗
        /// </summary>
        public bool DayFolder { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        ///     文本日志的路径,如果不配置,就为:[应用程序的路径]\log\
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// 拆分日志的数量
        /// </summary>
        public int SplitNumber { get; set; }

        /// <summary>
        /// 是否开启联机日志
        /// </summary>
        public bool MulitLog { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            try
            {
                if (SplitNumber <= 0)
                {
                    SplitNumber = 10;
                }

                SplitNumber <<= 20;
                if (MinFreeSize <= 0)
                {
                    MinFreeSize = 1;
                }

                if (MaxFile <= 0)
                {
                    MaxFile = 999;
                }

                if (string.IsNullOrWhiteSpace(LogPath))
                {
                    LogPath = LogRecorder.LogPath;
                    if (string.IsNullOrWhiteSpace(LogPath))
                    {
                        LogPath = System.IO.Path.Combine(Environment.CurrentDirectory, "logs");
                    }
                }
                IOHelper.CheckPath(LogPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), "TextRecorder.Initialize LogPath");
            }
            try
            {
                var size = IOHelper.FolderDiskInfo(LogPath);
                if (size.AvailableSize < MinFreeSize)
                {
                    Disable = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), "TextRecorder.Initialize LogPath");
            }
        }
    }
}