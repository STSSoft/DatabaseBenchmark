using STS.General.Diagnostics;
using System.Collections.Generic;

namespace DatabaseBenchmark.Statistics
{
    /// <summary>
    /// Accumulates memory usage statistics for a data flow.
    /// </summary>
    public class MemoryStatistics : IStatistic
    {
        private readonly MemoryMonitor monitor = new MemoryMonitor();

        private long count;

        private List<KeyValuePair<long, float>> pagedMemory;
        private List<KeyValuePair<long, float>> virtualMemory;
        private List<KeyValuePair<long, float>> workingSet;

        public int Step { get; set; }

        public MemoryStatistics(int capacity)
        {
            pagedMemory = new List<KeyValuePair<long, float>>(capacity);
            pagedMemory.Add(new KeyValuePair<long, float>(0, 0));

            virtualMemory = new List<KeyValuePair<long, float>>(capacity);
            virtualMemory.Add(new KeyValuePair<long, float>(0, 0));

            workingSet = new List<KeyValuePair<long, float>>(capacity);
            workingSet.Add(new KeyValuePair<long, float>(0, 0));
        }

        public MemoryStatistics()
            : this(100)
        {
        }

        /// <summary>
        /// Start monitoring the memory activity.
        /// </summary>
        public void Start()
        {
            monitor.Start();
        }

        /// <summary>
        /// Adds a statistic entry.
        /// </summary>
        public void Add()
        {
            count++;

            if (count % Step == 0)
            {
                pagedMemory.Add(new KeyValuePair<long, float>(count, monitor.AveragePagedMemory));
                virtualMemory.Add(new KeyValuePair<long, float>(count, monitor.AverageVirtualMemory));
                workingSet.Add(new KeyValuePair<long, float>(count, monitor.AverageWorkingSet));
            }
        }

        /// <summary>
        /// Stops monitoring the memory activity.
        /// </summary>
        public void Stop()
        {
            monitor.Stop();
        }

        public float PeakPagedMemory
        {
            get { return monitor.PeakPagedMemory; }
        }

        public float PeakVirtualMemory
        {
            get { return monitor.PeakVirtualMemory; }
        }

        public float PeakWorkingSet
        {
            get { return monitor.PeakWorkingSet; }
        }

        /// <summary>
        /// Gets the statistic entries for the average paged memory.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AveragePagedMemoryStats
        {
            get
            {
                foreach (var item in pagedMemory)
                    yield return item;
            }
        }

        /// <summary>
        /// Gets the statistic entries for the average virtual memory.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AverageVirtualMemoryStats
        {
            get
            {
                foreach (var item in virtualMemory)
                    yield return item;
            }
        }

        /// <summary>
        /// Gets the statistic entries for the average working set.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AverageWorkingSetStats
        {
            get
            {
                foreach (var item in workingSet)
                    yield return item;
            }
        }
    }
}
