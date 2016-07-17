using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace LiveRecordSharp
{
    public static class Log
    {
        private const string Pattern = "%utcdate{g} %-5level - %message%newline";

        public static ILog GetLogger(Type type)
        {
            var log = LogManager.GetLogger(type);

            var consoleAppender = new ConsoleAppender {Layout = new PatternLayout(Pattern)};
            consoleAppender.ActivateOptions();

            var errorFileAppender = new RollingFileAppender
            {
                Layout = new PatternLayout(Pattern),
                File = "error.log",
                CountDirection = 1,
                MaxSizeRollBackups = -1
            };
            errorFileAppender.AddFilter(new LevelRangeFilter {LevelMin = Level.Error, LevelMax = Level.Fatal});
            errorFileAppender.ActivateOptions();

            ((Logger) log.Logger).AddAppender(consoleAppender);
            ((Logger) log.Logger).AddAppender(errorFileAppender);
            var hierarchy = (Hierarchy) LogManager.GetRepository();
            hierarchy.Configured = true;
            return log;
        }
    }
}
