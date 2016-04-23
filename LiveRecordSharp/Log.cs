using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static ILog GetLogger(Type type)
        {
            var log = LogManager.GetLogger(type);

            var consoleAppender = new ConsoleAppender { Layout = new SimpleLayout() };

            var errorFileAppender = new FileAppender
            {
                Layout = new SimpleLayout(),
                File = "error.log"
            };
            errorFileAppender.AddFilter(new LevelRangeFilter { LevelMin = Level.Error, LevelMax = Level.Fatal });
            
            ((Logger)log.Logger).AddAppender(consoleAppender);
            ((Logger)log.Logger).AddAppender(errorFileAppender);
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Configured = true;
            return log;
        }
    }
}
