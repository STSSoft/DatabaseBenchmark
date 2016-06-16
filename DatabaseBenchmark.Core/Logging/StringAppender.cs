using log4net.Appender;
using log4net.Core;
using System;
using System.Text;

namespace DatabaseBenchmark.Core.Logging
{
    public class StringAppender : AppenderSkeleton
    {
        private StringBuilder Logs = new StringBuilder();

        public event Action OnAppend;
        public string LastLine { get; private set; }

        public StringAppender()
        {
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            string line = RenderLoggingEvent(loggingEvent);

            Logs.AppendLine(line);
            LastLine = line;

            if (OnAppend != null)
                OnAppend();
        }

        /// <summary>
        /// Gets the logged strings.
        /// </summary>
        /// <returns></returns>
        public string GetLogs()
        {
            return Logs.ToString();
        }

        /// <summary>
        /// Clears all logged data.
        /// </summary>
        public void Clear()
        {
            Logs.Clear();
        }

        /// <summary>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </summary>
        /// <value><c>true</c></value>
        protected override bool RequiresLayout
        {
            get { return true; }
        }
    }
}
