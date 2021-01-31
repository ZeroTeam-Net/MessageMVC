using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Agebull.Common.Logging
{

    /// <summary>
    /// A provider of <see cref="T:Microsoft.Extensions.Logging.Console.TextLogger" /> instances.
    /// </summary>
    public class MessageLoggerProvider : ILoggerProvider
    {
        private static readonly ConcurrentDictionary<string, MessageLogger> loggers = new ConcurrentDictionary<string, MessageLogger>();

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            if (loggers.TryGetValue(name, out var log))
                return log;
            loggers.TryAdd(name, new MessageLogger
            {
                LoggerName = name
            });
            return loggers[name];
        }

        void IDisposable.Dispose()
        {
        }
    }


}