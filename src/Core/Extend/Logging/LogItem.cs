#region

using System;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
#endregion

namespace Agebull.Common.Logging
{
    /// <summary>
    /// 日志内容
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// 日志标识
        /// </summary>
        public int LogId { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 记录器名称
        /// </summary>
        public string LoggerName { get; set; }
        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 当前异常
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// 当前服务
        /// </summary>
        public string Service { get; set; }
        /// <summary>
        /// 当前机器
        /// </summary>
        public string Machine { get; set; }

        /// <summary>
        /// 当前用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 接口名称
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// 消息跟踪ID
        /// </summary>
        public string TraceId { get; set; }
        /// <summary>
        /// 调用消息ID
        /// </summary>
        public string CallId { get; set; }

        /// <summary>
        /// 本地消息ID
        /// </summary>
        public string LocalId { get; set; }

    }
}