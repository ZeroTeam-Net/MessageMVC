// 所在工程：Agebull.EntityModel
// 整理用户：agebull
// 建立时间：2012-08-13 5:35
// 整理时间：2018年5月16日 00:34:00
#region

using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Agebull.Common.Logging
{
    /// <summary>
    ///   日志记录器
    /// </summary>
    public static partial class LogRecorder
    {
        #region 配置

        /// <summary>
        /// 日志序号
        /// </summary>
        internal static long lastId = 1;

        /// <summary>
        ///     文本日志的路径,如果不配置,就为:[应用程序的路径]\log\
        /// </summary>
        public static string LogPath { get; set; }

        /// <summary>
        /// 是否开启跟踪日志
        /// </summary>
        public static bool LogMonitor { get; set; }

        /// <summary>
        /// 跟踪日志是否包含详细信息
        /// </summary>
        public static bool MonitorIncludeDetails { get; set; }

        /// <summary>
        /// 是否开启SQL日志
        /// </summary>
        public static bool LogDataSql { get; set; }

        /// <summary>
        /// 取请求ID的方法
        /// </summary>
        public static Func<string> GetRequestIdFunc;

        /// <summary>
        /// 取得当前用户方法
        /// </summary>
        public static Func<string> GetUserNameFunc;

        /// <summary>
        /// 取请求ID的方法
        /// </summary>
        public static Func<string> GetMachineNameFunc;

        #endregion

        #region 流程

        /// <summary>
        ///   静态构造
        /// </summary>
        static LogRecorder()
        {
            Initialize();
        }
        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ReadConfig();
            if (ConfigurationManager.Root.GetSection("LogRecorder:noRegist")?.Value != "true")
            {
                DoInitialize();
            }
        }
        static int isInitialized;

        /// <summary>
        ///     初始化
        /// </summary>
        public static void DoInitialize()
        {
            if (Interlocked.Increment(ref isInitialized) > 1)
                return;
            DependencyHelper.ServiceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(ConfigurationManager.Root.GetSection("Logging"));
                if (ConfigurationManager.Root.GetSection("LogRecorder:innerLogger")?.Value != "true")
                {
                    return;
                }

                builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());
                LoggerProviderOptions.RegisterProviderOptions<TextLoggerOption, TextLoggerProvider>(builder.Services);

            });
            DependencyHelper.Update();
            ConfigurationManager.RegistOnChange("LogRecorder", ReadConfig, false);
        }
        /// <summary>
        /// 读取配置
        /// </summary>
        private static void ReadConfig()
        {
            var sec = ConfigurationManager.Get("LogRecorder");
            if (sec != null)
            {
                MonitorIncludeDetails = sec.GetBool("details");
                LogDataSql = sec.GetBool("sql");
                LogMonitor = sec.GetBool("monitor");
            }
#if !NETCOREAPP
            if (LogMonitor)
            {
                AppDomain.MonitoringIsEnabled = true;
            }
#endif
        }

        #endregion

        #region 支持

        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger Logger => DependencyScope.Logger;

        /// <summary>
        /// 取请求ID
        /// </summary>
        public static string GetRequestId()
        {
            return GetRequestIdFunc?.Invoke() ?? RandomCode.Generate(8);
        }

        /// <summary>
        /// 取得当前用户
        /// </summary>
        public static string GetUserName()
        {
            return GetUserNameFunc?.Invoke() ?? "*";
        }
        /// <summary>
        /// 取得当前机器
        /// </summary>
        public static string GetMachineName()
        {
            return GetMachineNameFunc?.Invoke() ?? "Local";
        }

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

        #endregion

        #region 记录

        ///<summary>
        ///  记录数据日志
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>

        public static void RecordDataLog(string message)
        {
            if (LogDataSql)
            {
                if (message == null)
                {
                    return;
                }
                var eventId = new EventId((int)Interlocked.Increment(ref lastId), "DataLog");
                Logger.LogTrace(eventId, message);
            }
        }

        ///<summary>
        ///  记录消息
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Information(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Information");
            Logger.LogInformation(eventId, message, formatArgs);
        }

        /// <summary>
        ///   记录系统日志
        /// </summary>
        /// <param name="message"> 消息 </param>
        public static void Information(string message)
        {
            if (message == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Information");
            Logger.LogInformation(eventId, message);
        }

        ///<summary>
        ///  记录警告消息
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Warning(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Warning");
            Logger.LogWarning(eventId, message, formatArgs);
        }

        ///<summary>
        ///  记录提示消息
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        public static void Error(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Error");
            Logger.LogError(eventId, message, formatArgs);
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="exception"> 异常 </param>
        /// <param name="message"> 日志详细信息 </param>
        public static string Exception(Exception exception, string message = null)
        {
            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Exception");
            Logger.LogError(eventId, exception, message);
            return exception.Message;
        }

        /// <summary>
        ///   记录异常日志
        /// </summary>
        /// <param name="ex"> 异常 </param>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="formatArgs">格式化参数</param>
        public static string Exception(Exception ex, string message, params object[] formatArgs)
        {
            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Exception");
            Logger.LogError(eventId, ex, message, formatArgs);
            return ex.Message;
        }
        #endregion

        #region 跟踪

        /// <summary>
        ///   记录堆栈跟踪
        /// </summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        [Conditional("Trace")]
        public static void RecordStackTrace(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }
            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Trace");
            Logger.LogTrace(eventId, StackTraceInfomation(message, formatArgs));
        }

        /// <summary>
        /// 写入跟踪日志
        /// </summary>
        [Conditional("Trace")]
        public static void Trace(Func<string> message)
        {
            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Trace");
            Logger.LogTrace(eventId, message());
        }

        /// <summary>
        ///   写入跟踪日志
        /// </summary>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="formatArgs">格式化参数</param>
        [Conditional("Trace")]
        public static void Trace(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }
            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Trace");
            Logger.LogTrace(eventId, message, formatArgs);
        }

        #endregion

        #region 调试

        ///<summary>
        ///  写入调试日志同时记录堆栈信息
        ///</summary>
        ///<param name="message"> 日志详细信息 </param>
        ///<param name="formatArgs"> 格式化的参数 </param>
        [Conditional("Debug")]
        public static void DebugByStackTrace(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Debug");
            Logger.LogDebug(eventId, StackTraceInfomation(message, formatArgs));
        }

        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="message"> 日志详细信息 </param>
        /// <param name="formatArgs">格式化参数</param>
        [Conditional("Debug")]
        public static void Debug(string message, params object[] formatArgs)
        {
            if (message == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Debug");
            Logger.LogDebug(eventId, message, formatArgs);
        }
        /// <summary>
        ///   写入调试日志
        /// </summary>
        /// <param name="obj"> 记录对象 </param>
        [Conditional("Debug")]
        public static void Debug(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var eventId = new EventId((int)Interlocked.Increment(ref lastId), "Debug");
            Logger.LogDebug(eventId, obj?.ToString());
        }

        #endregion
    }
}