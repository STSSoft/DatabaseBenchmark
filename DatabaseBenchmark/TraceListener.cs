using DatabaseBenchmark.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark
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
