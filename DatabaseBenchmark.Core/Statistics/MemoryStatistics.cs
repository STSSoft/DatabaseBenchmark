using STS.General.Diagnostics;
using System.Collections.Generic;

namespace DatabaseBenchmark.Core.Statistics
{
    /// <summary>
    /// Accumulates memory usage statistics for a data flow.
    /// </summary>
    public class MemoryStatistics : IStatistic
    {
        private readonly MemoryMonitor memoryMonitor = new MemoryMonitor();

        private long count;

        private List<KeyValuePair<long, float>> pagedMemory;
        private List<KeyValuePair<long, float>> virtualMemory;
        private List<KeyValuePair<long, float>> workingSet;

        public int Step { get; set; }

        public MemoryStatistics(int capacity, int step)
        {
            pagedMemory = new List<KeyValuePair<long, float>>(capacity);
            pagedMemory.Add(new KeyValuePair<long, float>(0, 0));

            virtualMemory = new List<KeyValuePair<long, float>>(capacity);
            virtualMemory.Add(new KeyValuePair<long, float>(0, 0));

            workingSet = new List<KeyValuePair<long, float>>(capacity);
            workingSet.Add(new KeyValuePair<long, float>(0, 0));

            Step = step;
        }

        public MemoryStatistics(int capacity)
            : this(capacity, 1)
        {
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
            memoryMonitor.Start();
        }

        /// <summary>
        /// Stops monitoring the memory activity.
        /// </summary>
        public void Stop()
        {
            memoryMonitor.Stop();
        }

        /// <summary>
        /// Adds a statistic entry.
        /// </summary>
        public void Add()
        {
            count++;

            if (count % Step == 0)
            {
                pagedMemory.Add(new KeyValuePair<long, float>(count, memoryMonitor.PagedMemory));
                virtualMemory.Add(new KeyValuePair<long, float>(count, memoryMonitor.VirtualMemory));
                workingSet.Add(new KeyValuePair<long, float>(count, memoryMonitor.WorkingSet));
            }
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        public void Reset()
        {
            count = 0;

            pagedMemory.Clear();
            virtualMemory.Clear();
            workingSet.Clear();

            memoryMonitor.Reset();
        }

        public float PeakPagedMemory
        {
            get { return memoryMonitor.PeakPagedMemory; }
        }

        public float PeakVirtualMemory
        {
            get { return memoryMonitor.PeakVirtualMemory; }
        }

        public float PeakWorkingSet
        {
            get { return memoryMonitor.PeakWorkingSet; }
        }

        /// <summary>
        /// Gets the statistic entries for the average paged memory.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> MomentPagedMemoryStats
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
        public IEnumerable<KeyValuePair<long, float>> MomentVirtualMemoryStats
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
        public IEnumerable<KeyValuePair<long, float>> MomentWorkingSetStats
        {
            get
            {
                foreach (var item in workingSet)
                    yield return item;
            }
        }
    }
}
