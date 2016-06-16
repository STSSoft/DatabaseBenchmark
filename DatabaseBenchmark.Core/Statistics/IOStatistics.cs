using STS.General.Diagnostics;
using System.Collections.Generic;

namespace DatabaseBenchmark.Core.Statistics
{
    /// <summary>
    /// Accumulates I/O usage statistics for a data flow.
    /// </summary>
    public class IOStatistics : IStatistic
    {
        private readonly IOMonitor ioMonitor = new IOMonitor();

        private long count;

        private List<KeyValuePair<long, float>> writeIO;
        private List<KeyValuePair<long, float>> readIO;
        private List<KeyValuePair<long, float>> dataIO;

        public int Step { get; set; }

        public IOStatistics(int capacity)
        {
            writeIO = new List<KeyValuePair<long, float>>(capacity);
            writeIO.Add(new KeyValuePair<long, float>(0, 0));

            readIO = new List<KeyValuePair<long, float>>(capacity);
            readIO.Add(new KeyValuePair<long, float>(0, 0));

            dataIO = new List<KeyValuePair<long, float>>(capacity);
            dataIO.Add(new KeyValuePair<long, float>(0, 0));
        }

        public IOStatistics()
            : this(100)
        {
        }

        /// <summary>
        /// Start monitoring the I/O activity.
        /// </summary>
        public void Start()
        {
            ioMonitor.Start();
        }

        /// <summary>
        /// Stops monitoring the I/O activity.
        /// </summary>
        public void Stop()
        {
            ioMonitor.Stop();
        }

        /// <summary>
        /// Adds a statistic entry.
        /// </summary>
        public void Add()
        {
            count++;

            if (count % Step == 0)
            {
                writeIO.Add(new KeyValuePair<long, float>(count, ioMonitor.IOWriteBytes));
                readIO.Add(new KeyValuePair<long, float>(count, ioMonitor.IOReadBytes));
                dataIO.Add(new KeyValuePair<long, float>(count, ioMonitor.IODataBytes));
            }
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        public void Reset()
        {
            count = 0;

            writeIO.Clear();
            readIO.Clear();
            dataIO.Clear();

            ioMonitor.Reset();
        }

        public float MomentIOWrite
        {
            get { return ioMonitor.IOWriteBytes; }
        }

        public float MomentIORead
        {
            get { return ioMonitor.IOReadBytes; }
        }

        public float MomentIOData
        {
            get { return ioMonitor.IODataBytes; }
        }

        /// <summary>
        /// Gets the statistic entries for the write I/O.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> MomentWriteIOStats
        {
            get
            {
                foreach (var item in writeIO)
                    yield return item;
            }
        }

        /// <summary>
        /// Gets the statistic entries for the read I/O.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> MomentReadIOStats
        {
            get
            {
                foreach (var item in readIO)
                    yield return item;
            }
        }

        /// <summary>
        /// Gets the statistic entries for data I/O.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> MomentDataIOStats
        {
            get
            {
                foreach (var item in dataIO)
                    yield return item;
            }
        }
    }
}
