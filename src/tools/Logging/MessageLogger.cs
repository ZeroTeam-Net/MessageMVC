// 所在工程：Agebull.EntityModel
// 整理用户：bull2
// 建立时间：2012-08-13 5:35
// 整理时间：2018年6月12日, AM 12:25:44

#region

using Agebull.Common.Base;
using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
            /*if (ScopeRuner.InScope)
            {
                var message = GlobalContext.Current?.Message;
                if (message != null)
                {
                    item.MessageId = message.ID;
                    item.ApiName = $"{message.Service}/{message.Method}"; 
                    if (message.Trace != null)
                    {
                        item.LocalId = message.Trace.LocalId;
                        item.CallId = message.Trace.CallId;
                        item.TraceId = message.Trace.TraceId;
                    }
                }
                var user = GlobalContext.User;
                if(user != null)
                    item.UserId = user.UserId;
            }*/
            await MessagePoster.PublishAsync(LogOption.Instance.Service, "text", new LogItem
            {
                Time = DateTime.Now,
                LogLevel = logLevel,
                LogId = eventId.Id,
                LoggerName = LoggerName,
                Message = state?.ToString(),
                Exception = exception?.ToString()
            });
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