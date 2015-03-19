using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;

namespace DatabaseBenchmark
{
    public class StringAppender : AppenderSkeleton
    {
        private StringBuilder Logs = new StringBuilder();

        public StringAppender()
		{
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
		/// Resets the appender.
		/// </summary>
		public void Reset()
		{
            Logs.Clear();
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			Logs.Append(RenderLoggingEvent(loggingEvent));
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
