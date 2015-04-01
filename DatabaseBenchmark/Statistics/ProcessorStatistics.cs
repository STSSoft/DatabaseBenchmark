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
        private readonly CPUMonitor cpuMonitor = new CPUMonitor();

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
        /// Stops monitoring the processor activity.
        /// </summary>
        public void Stop()
        {
            cpuMonitor.Stop();
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
                    privilegedTime.Add(new KeyValuePair<long, float>(count, cpuMonitor.PrivilegedTime));
                    processorTime.Add(new KeyValuePair<long, float>(count, cpuMonitor.ProcessorTime));
                    userTime.Add(new KeyValuePair<long, float>(count, cpuMonitor.UserTime));
                }
            }
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        public void Reset()
        {
            count = 0;

            privilegedTime.Clear();
            processorTime.Clear();
            userTime.Clear();

            cpuMonitor.Reset();
        }
     
        public float MomentPrivilegedTime
        {
            get
            {
                lock (SyncRoot)
                    return cpuMonitor.PrivilegedTime;
            }
        }

        public float MomentProcessorTime
        {
            get
            {
                lock (SyncRoot)
                    return cpuMonitor.ProcessorTime;
            }
        }

        public float MomentUserTime
        {
            get
            {
                lock (SyncRoot)
                    return cpuMonitor.UserTime;
            }
        }

        /// <summary>
        /// Gets the statistic entries for the average privileged time.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> MomentPrivilegedTimeStats
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
        public IEnumerable<KeyValuePair<long, float>> MomentProcessorTimeStats
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
        public IEnumerable<KeyValuePair<long, float>> MomentUserTimeStats
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
