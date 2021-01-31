// 所在工程：Agebull.EntityModel
// 整理用户：agebull
// 建立时间：2012-08-13 5:35
// 整理时间：2018年5月16日 00:34:00
#region

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

        ///<summary>
        ///  记录日志
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="level"> 日志详细信息 </param>
        ///<param name="func"> 格式化的参数 </param>
        public static void Log(this ILogger logger, LogLevel level,Func<string> func)
        {
            if (logger != null && logger.IsEnabled(level))
                logger.Log(level, func());
        }


        ///<summary>
        ///  记录警告消息
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Warning(this ILogger logger, string message, params object[] formatArgs)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                logger.Log(LogLevel.Warning, message, formatArgs);
        }

        ///<summary>
        ///  记录警告消息
        ///</summary>
        /// <param name="logger">日志记录器</param>
        ///<param name="message"> 日志详细信息 </param>
        public static void Warning(this ILogger logger, string message)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                logger.Log(LogLevel.Warning, message);
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
                logger.Log(LogLevel.Error,  message, formatArgs);
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
                logger.Log(LogLevel.Error,  exception, message);
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
                logger.Log(LogLevel.Error,  ex, message, formatArgs);
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
                logger.Log(LogLevel.Trace, message, formatArgs);
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
                logger.Log(LogLevel.Debug, StackTraceInfomation(message, formatArgs));
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
                logger.Log(LogLevel.Debug, message, formatArgs);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="obj"> 记录对象 </param>

        public static void Debug(this ILogger logger, object obj)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, obj?.ToString());
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
                logger.Log(LogLevel.Trace, func());
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Debug(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, func());
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Information(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Information))
                logger.Log(LogLevel.Information, func());
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
                logger.Log(LogLevel.Information, message, formatArgs);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="msg"> 消息</param>

        public static void Information(this ILogger logger, string msg)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Information))
                logger.Log(LogLevel.Information, msg);
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Warning(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Warning))
                logger.Log(LogLevel.Warning, func());
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="func"> 消息方法</param>

        public static void Error(this ILogger logger, Func<string> func)
        {
            if (logger != null && logger.IsEnabled(LogLevel.Error))
                logger.Log(LogLevel.Error, func());
        }
        #endregion
    }
}