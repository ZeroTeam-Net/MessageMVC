// 所在工程：Agebull.EntityModel
// 整理用户：bull2
// 建立时间：2012-08-13 5:35
// 整理时间：2018年6月12日, AM 12:25:44

#region

using Agebull.Common.Base;
using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
#endregion

namespace Agebull.Common.Logging
{
    /// <summary>
    ///     文本记录器
    /// </summary>
    internal sealed class MessageLogger : ILogger
    {
        internal string LoggerName { get; set; }

        async void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (ZeroAppOption.Instance.IsClosed || !LogOption.Instance.Enable || logLevel < LogOption.Instance.Level)
            {
                return;
            }
            try
            {
                var item = new LogItem
                {
                    Time = DateTime.Now,
                    LogLevel = logLevel,
                    LogId = eventId.Id,
                    LoggerName = LoggerName,
                    Message = state?.ToString(),
                    Exception = exception?.ToString(),
                    Service = ZeroAppOption.Instance.LocalApp,
                    Machine = ZeroAppOption.Instance.LocalMachine
                };
                if (ScopeRuner.InScope)
                {
                    item.UserId = GlobalContext.User?.UserId;
                    var message = GlobalContext.Current?.Message;
                    if (message != null)
                    {
                        item.MessageId = message.ID;
                        item.ApiName = $"{message.Service}/{message.Method}";
                        if (message.TraceInfo != null)
                        {
                            item.TraceId = message.TraceInfo.TraceId;
                            item.LocalId = message.TraceInfo.LocalId;
                            item.CallId = message.TraceInfo.CallId;
                        }
                    }
                }
                await MessagePoster.PublishAsync(LogOption.Instance.Service, "text", item);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
            }
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        /// <summary>
        /// 范围
        /// </summary>
        public IExternalScopeProvider ScopeProvider { get; set; }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return ScopeProvider?.Push(state) ?? new EmptyScope();
        }

    }
}