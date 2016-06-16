using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Core.Reporting
{
    public class PerformanceReport
    {
        /// <summary>
        /// The total elapsed time of the test.
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// The average speed of the test.
        /// </summary>
        public double AverageSpeed { get; set; }

        /// <summary>
        /// Total number of records used for the test.
        /// </summary>
        public long RecordCount { get; set; }

        /// <summary>
        /// Peak used memory (RAM).
        /// </summary>
        public float PeakWorkingSet { get; set; }

        public PerformanceReport(TimeSpan time, double averageSpeed, long recordCount, float peakWorkingSet)
        {
            ElapsedTime = time;
            AverageSpeed = averageSpeed;
            RecordCount = recordCount;
            PeakWorkingSet = peakWorkingSet;
        }

        public PerformanceReport()
        {
        }
    }
}
