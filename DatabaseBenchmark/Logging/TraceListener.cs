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
    /// <summary>
    /// TraceListener implemented over Log4Net logger object.
    /// </summary>
    public class Log4NetTraceListener : TraceListener
    {
        private ILog Logger;

        public Log4NetTraceListener(string loggerName)
        {
            Logger = log4net.LogManager.GetLogger(loggerName);
        }

        public Log4NetTraceListener(ILog logger)
        {
            Logger = logger;
        }

        public override void Fail(string message)
        {
            if(Logger != null)
            {
                Logger.Fatal(message);
            }
        }

        /// <summary>
        /// Traces the message. Supported event types are: Critical, Error, Information and Warning.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (Logger != null)
            {
                switch (eventType)
                {
                    case TraceEventType.Critical:
                        {
                            Logger.Fatal(message);
                        }
                        break;

                    case TraceEventType.Error:
                        {
                            Logger.Error(message);
                        }
                        break;

                    case TraceEventType.Information:
                        {
                            Logger.Info(message);
                        }
                        break;

                    case TraceEventType.Warning:
                        {
                            Logger.Warn(message);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Traces the message. Supported event types are: Critical, Error, Information and Warning.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id);
        }

        public override void Write(object o)
        {
            if (Logger != null)
            {
                Logger.Info(o);
            }
        }

        public override void Write(string message, string category)
        {
            Write(message, String.Empty);
        }

        /// <summary>
        /// Writes the message to the Log4net object. The message will be logged as
        /// an INFO.
        /// </summary>
        public override void Write(string message)
        {
            if (Logger != null)
            {
                Logger.Info(message);
            }
        }

        public override void WriteLine(object o)
        {
            if (Logger != null)
            {
                Logger.Info(o);
            }
        }

        public override void Write(object o, string category)
        {
            Write(o);
        }

        /// <summary>
        /// Writes the message to the Log4net object. The message will be logged as
        /// an INFO.
        /// </summary
        public override void WriteLine(string message)
        {
            if (Logger != null)
            {
                Logger.Info(message);
            }
        }

        public override void WriteLine(string message, string category)
        {
            WriteLine(message);
        }

        public override void WriteLine(object o, string category)
        {
            if (Logger != null)
            {
                Logger.Info(o);
            }
        }
    }
}
