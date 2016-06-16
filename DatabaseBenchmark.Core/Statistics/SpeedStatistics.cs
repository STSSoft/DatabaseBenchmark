using STS.General.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseBenchmark.Core.Statistics
{
    /// <summary>
    /// Accumulates simple speed statistic for a data flow. This class is thread-safe.
    /// </summary>
    public class SpeedStatistics : IStatistic
    {
        private readonly Stopwatch sw = new Stopwatch();

        private long count;
        // Records -> elapsed time.
        private List<KeyValuePair<long, TimeSpan>> recordTimeStat;

        public int Step { get; set; }

        public SpeedStatistics(int capacity, int step)
        {
            recordTimeStat = new List<KeyValuePair<long, TimeSpan>>(capacity);
            Step = step;
        }

        public SpeedStatistics(int capacity)
            : this(capacity, 1)
        {
        }

        public SpeedStatistics()
            : this(100)
        {
        }

        /// <summary>
        /// Starts the stopwatch.
        /// </summary>
        public void Start()
        {
            sw.Start();
        }

        /// <summary>
        /// Stops the stopwatch.
        /// </summary>
        public void Stop()
        {
            sw.Stop();
        }

        public void Add()
        {
            count++;

            if (count % Step == 0)
            {
                TimeSpan currentElapsed = sw.Elapsed;
                recordTimeStat.Add(new KeyValuePair<long, TimeSpan>(count, currentElapsed));
            }
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        public void Reset()
        {
            count = 0;

            recordTimeStat.Clear();
            sw.Reset();
        }

        public KeyValuePair<long, TimeSpan>[] RecordTime
        {
            get
            {
                return recordTimeStat.ToArray();
            }
        }

        /// <summary>
        /// Gets the total elapsed time of the stopwatch.
        /// </summary>
        public TimeSpan Time
        {
            get
            {
                return sw.Elapsed;
            }
        }

        /// <summary>
        /// Gets the speed according to the elapsed time of the stopwatch.
        /// </summary>
        public double Speed
        {
            get
            {
                return sw.GetSpeed(count);
            }
        }

        /// <summary>
        /// Gets the current number of records.
        /// </summary>
        public long Count
        {
            get
            {
                return count;
            }
        }

        public double GetAverageSpeedAt(int index)
        {
            if (index >= recordTimeStat.Count)
                return -1;

            var curr = recordTimeStat[index];

            return curr.Key / curr.Value.TotalSeconds;
        }

        public double GetMomentSpeedAt(int index)
        {
            if (index >= recordTimeStat.Count)
                return -1;

            var current = recordTimeStat[index];

            if (index == 0)
                return current.Key / current.Value.TotalSeconds;

            var previous = recordTimeStat[index - 1];

            return (current.Key - previous.Key) / (current.Value - previous.Value).TotalSeconds;
        }

        public KeyValuePair<long, TimeSpan> GetRecordAt(int index)
        {
            if (index >= recordTimeStat.Count)
                return new KeyValuePair<long, TimeSpan>(-1, TimeSpan.Zero);

            return recordTimeStat[index];
        }
    }
}
