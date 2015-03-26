using STS.General.Diagnostics;
using System.Collections.Generic;

namespace DatabaseBenchmark.Statistics
{
    /// <summary>
    /// Accumulate processor usage statistics for a data flow.
    /// </summary>
    public class ProcessorStatistics : IStatistic
    {
        private readonly object SyncRoot = new object();
        private readonly ProcessorMonitor cpuMonitor = new ProcessorMonitor();

        private long count;

        private List<KeyValuePair<long, float>> privilegedTime;
        private List<KeyValuePair<long, float>> processorTime;
        private List<KeyValuePair<long, float>> userTime;

        public int Step { get; set; }

        public ProcessorStatistics(int capacity)
        {
            privilegedTime = new List<KeyValuePair<long, float>>(capacity);
            privilegedTime.Add(new KeyValuePair<long, float>(0, 0));

            processorTime = new List<KeyValuePair<long, float>>(capacity);
            processorTime.Add(new KeyValuePair<long, float>(0, 0));

            userTime = new List<KeyValuePair<long, float>>(capacity);
            userTime.Add(new KeyValuePair<long, float>(0, 0));
        }

        public ProcessorStatistics()
            : this(100)
        {
        }

        /// <summary>
        /// Start monitoring the processor activity.
        /// </summary>
        public void Start()
        {
            cpuMonitor.Start();
        }

        /// <summary>
        /// Adds a statistic entry.
        /// </summary>
        public void Add()
        {
            lock (SyncRoot)
            {
                count++;

                if (count % Step == 0)
                {
                    privilegedTime.Add(new KeyValuePair<long, float>(count, cpuMonitor.AveragePrivilegedTimePercent));
                    processorTime.Add(new KeyValuePair<long, float>(count, cpuMonitor.AverageProcessorTimePercent));
                    userTime.Add(new KeyValuePair<long, float>(count, cpuMonitor.AverageUserTimePercent));
                }
            }
        }

        /// <summary>
        /// Stops monitoring the processor activity.
        /// </summary>
        public void Stop()
        {
            cpuMonitor.Stop();
        }

        public float AveragePrivilegedTime
        {
            get
            {
                lock (SyncRoot)
                    return cpuMonitor.AveragePrivilegedTimePercent;
            }
        }

        public float AverageProcessorTime
        {
            get
            {
                lock (SyncRoot)
                    return cpuMonitor.AverageProcessorTimePercent;
            }
        }

        public float AverageUserTime
        {
            get
            {
                lock (SyncRoot)
                    return cpuMonitor.AverageUserTimePercent;
            }
        }

        /// <summary>
        /// Gets the statistic entries for the average privileged time.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AveragePrivilegedTimeStats
        {
            get
            {
                lock (SyncRoot)
                {
                    foreach (var item in privilegedTime)
                        yield return item;
                }
            }
        }

        /// <summary>
        /// Gets the statistic entries for the average processor time.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AverageProcessorTimeStats
        {
            get
            {
                lock (SyncRoot)
                {
                    foreach (var item in processorTime)
                        yield return item;
                }
            }
        }

        /// <summary>
        /// Gets the statistic entries for the user time.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AverageUserTimeStats
        {
            get
            {
                lock (SyncRoot)
                {

                    foreach (var item in userTime)
                        yield return item;
                }
            }
        } 
    }
}
