using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Core.Reporting
{
    /// <summary>
    /// Provides simple statistics - elapsed time, average speed, moment speed as well
    /// as memory usage - paged, virtual and working set.
    /// </summary>
    public class PerformanceWatch
    {
        /// <summary>
        /// Invoked when the report is started.
        /// </summary>
        public event Action<PerformanceWatch> OnStart;

        /// <summary>
        /// Invoked when the report is stopped.
        /// </summary>
        public event Action<PerformanceWatch> OnStop;

        private string name;

        public string Name { get { return name; } }
        public int Step { get; private set; }

        public SpeedStatistics SpeedStatistics { get; set; }
        public MemoryStatistics MemoryStatistics { get; set; }

        public PerformanceWatch(string name, int step)
        {
            this.name = name;

            SpeedStatistics = new SpeedStatistics(Benchmark.INTERVAL_COUNT, step);
            MemoryStatistics = new MemoryStatistics(Benchmark.INTERVAL_COUNT, step);
        }

        public PerformanceWatch()
            : this(String.Empty, 1)
        {
        }

        /// <summary>
        /// Start all statistics.
        /// </summary>
        public void Start()
        {
            SpeedStatistics.Start();
            MemoryStatistics.Start();

            if (OnStart != null)
                OnStart(this);
        }

        /// <summary>
        /// Stop all statistics.
        /// </summary>
        public void Stop()
        {
            SpeedStatistics.Stop();
            MemoryStatistics.Stop();

            if (OnStop != null)
                OnStop(this);
        }

        /// <summary>
        /// Add point (measurement) to all statistics.
        /// </summary>
        public void Add()
        {
            SpeedStatistics.Add();
            MemoryStatistics.Add();
        }

        public TimeSpan GetElapsedTime()
        {
            return SpeedStatistics.Time;
        }

        public double GetAverageSpeed()
        {
            return SpeedStatistics.Speed;
        }

        public long GetRecordsCount()
        {
            return SpeedStatistics.Count;
        }

        public float GetPeakWorkingSet()
        {
            return MemoryStatistics.PeakWorkingSet;
        }
    }
}
