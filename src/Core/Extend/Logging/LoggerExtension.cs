// 所在工程：Agebull.EntityModel
// 整理用户：agebull
// 建立时间：2012-08-13 5:35
// 整理时间：2018年5月16日 00:34:00
#region

using Agebull.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Agebull.Common.Logging
{
    /// <summary>
    ///   日志记录器
    /// </summary>
    public static class LoggerExtension
    {
        #region 记录

        /// <summary>
        /// 日志序号
        /// </summary>
        static long lastId = 1;

        /// <summary>
        /// 新的事件ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EventId NewEventId(string name) => new EventId((int)Interlocked.Increment(ref lastId), name);

        ///<summary>
        ///  记录警告消息
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Warning(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                logger.Log(LogLevel.Warning, NewEventId("Warning"), message, formatArgs);
        }

        ///<summary>
        ///  记录警告消息
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 日志详细信息 </param>
        public static void Warning(this ILogger logger, string message)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                logger.Log(LogLevel.Warning, NewEventId("Warning"), message);
        }

        ///<summary>
        ///  记录错误消息
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Error(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Error))
                logger.Log(LogLevel.Error, NewEventId("Error"), message, formatArgs);
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="exception"> 异常 </param>
        /// <param name="message"> 日志详细信息 </param>
        public static string Exception(this ILogger logger, Exception exception, string message = null)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Error))
                logger.Log(LogLevel.Error, NewEventId("Exception"), exception, message);
            return exception.Message;
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="ex"> 异常 </param>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="formatArgs">格式化参数</param>
        public static string Exception(this ILogger logger, Exception ex, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Error))
                logger.Log(LogLevel.Error, NewEventId("Exception"), ex, message, formatArgs);
            return ex.Message;
        }

        #endregion

        #region 跟踪

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="formatArgs">格式化参数</param>

        public static void Trace(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Trace))
                logger.Log(LogLevel.Trace, NewEventId("Trace"), message, formatArgs);
        }
        #endregion

        #region 调试

        /// <summary>
        ///   堆栈信息
        /// </summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static string StackTraceInfomation(string message, object[] formatArgs)
        {
            if (message == null)
            {
                return new StackTrace().ToString();
            }
            if (formatArgs != null && formatArgs.Length != 0)
            {
                message = string.Format(message, formatArgs);
            }
            return $"{message}:\r\n{new StackTrace()}";
        }

        ///<summary>
        ///  写入调试日志同时记录堆栈信息
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void DebugByStackTrace(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, NewEventId("Debug"), StackTraceInfomation(message, formatArgs));
        }

        /// <summary>
        ///   写入调试日志.
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="formatArgs">格式化参数</param>
        public static void Debug(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, NewEventId("Debug"), message, formatArgs);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="obj"> 记录对象 </param>

        public static void Debug(this ILogger logger, object obj)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, NewEventId("Debug"), obj?.ToString());
        }

        ///<summary>
        ///  记录一般日志
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="name"> </param>
        ///<param name="message"> 消息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Debug(this ILogger logger, string name, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, NewEventId(name), message, formatArgs);
        }

        #endregion

        #region 方法

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Trace(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Trace))
                logger.Log(LogLevel.Trace, NewEventId("Information"), func());
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Debug(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, NewEventId("Information"), func());
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Information(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Information))
                logger.Log(LogLevel.Information, NewEventId("Information"), func());
        }

        ///<summary>
        ///  记录一般日志
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 消息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>

        public static void Information(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Information))
                logger.Log(LogLevel.Information,NewEventId("Information"), message, formatArgs);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="msg"> 消息</param>

        public static void Information(this ILogger logger, string msg)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Information))
                logger.Log(LogLevel.Information,NewEventId("Information"), msg);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Warning(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                logger.Log(LogLevel.Warning, NewEventId("Information"), func());
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Error(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Error))
                logger.Log(LogLevel.Error, NewEventId("Information"), func());
        }
        #endregion
    }
}