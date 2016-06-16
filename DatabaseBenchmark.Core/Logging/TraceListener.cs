using DatabaseBenchmark.Core.Properties;
using log4net;
using System.Diagnostics;

namespace DatabaseBenchmark.Core.Logging
{
    public class Log4NetTraceListener : TraceListener
    {
        private ILog Logger;

        public Log4NetTraceListener()
        {
            Logger = log4net.LogManager.GetLogger(Settings.Default.TestLogger);
        }

        public Log4NetTraceListener(ILog logger)
        {
            Logger = logger;
        }

        public override void Write(string message)
        {
            if (Logger != null)
            {
                Logger.Debug(message);
            }
        }

        public override void WriteLine(string message)
        {
            if (Logger != null)
            {
                Logger.Debug(message);
            }
        }
    }
}
