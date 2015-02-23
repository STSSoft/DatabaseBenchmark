using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STS.General.Diagnostics;

namespace DatabaseBenchmark.Statistics
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
        /// Adds a statistic entry.
        /// </summary>
        public void Add()
        {
            count++;

            if (count % Step == 0)
            {
                writeIO.Add(new KeyValuePair<long, float>(count, ioMonitor.AverageIOWriteBytes));
                readIO.Add(new KeyValuePair<long, float>(count, ioMonitor.AverageIOReadBytes));
                dataIO.Add(new KeyValuePair<long, float>(count, ioMonitor.AverageIODataBytes));
            }
        }

        /// <summary>
        /// Stops monitoring the I/O activity.
        /// </summary>
        public void Stop()
        {
            ioMonitor.Stop();
        }

        public float AverageIOWrite
        {
            get { return ioMonitor.AverageIOWriteBytes; }
        }

        public float AverageIORead
        {
            get { return ioMonitor.AverageIOReadBytes; }
        }

        public float AverageIOData
        {
            get { return ioMonitor.AverageIODataBytes; }
        }

        /// <summary>
        /// Gets the statistic entries for the write I/O.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> AverageWriteIOStats
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
        public IEnumerable<KeyValuePair<long, float>> AverageReadIOStats
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
        public IEnumerable<KeyValuePair<long, float>> AverageDataIOStats
        {
            get
            {
                foreach (var item in dataIO)
                    yield return item;
            }
        } 
    }
}
