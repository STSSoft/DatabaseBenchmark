using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseBenchmark.Statistics;
using STS.General.Generators;

namespace DatabaseBenchmark.Benchmarking
{
    /// <summary>
    /// Represents a benchmark test session for a single database.
    /// </summary>
    public class BenchmarkTest
    {
        public const int INTERVAL_COUNT = 100; // Gives the maximum number of intervals measured by the statistic.

        public SpeedStatistics[] SpeedStatistics { get; private set; }
        public ProcessorStatistics[] ProcessorStatistics { get; private set; }
        public MemoryStatistics[] MemoryStatistics { get; private set; }
        public IOStatistics[] IOStatistics { get; private set; }

        public TestMethod CurrentMethod { get; private set; }
        public Database Database { get; private set; }

        public long RecordCount { get; private set; }
        public int FlowCount { get; private set; }
       
        public float Randomness { get; private set; }
        public KeysType KeysType { get; private set; }

        public long DatabaseSize { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        private CancellationTokenSource Cancellation;

        public BenchmarkTest(Database database, int flowCount, long recordCount, float randomness, CancellationTokenSource cancellation)
        {
            Database = database;

            FlowCount = flowCount;
            RecordCount = recordCount;
            Randomness = randomness;
            KeysType =  Randomness == 0f ? KeysType.Sequential : KeysType.Random;

            // Speed, Memory and I/O statistics.
            int length = Enum.GetValues(typeof(TestMethod)).Length - 1;

            SpeedStatistics = new SpeedStatistics[length];
            ProcessorStatistics = new ProcessorStatistics[length];
            MemoryStatistics = new MemoryStatistics[length];
            IOStatistics = new IOStatistics[length];

            for (int i = 0; i < length; i++)
            {
                SpeedStatistics[i] = new SpeedStatistics(INTERVAL_COUNT);
                SpeedStatistics[i].Step = (int)((recordCount) / INTERVAL_COUNT);

                ProcessorStatistics[i] = new ProcessorStatistics(INTERVAL_COUNT);
                ProcessorStatistics[i].Step = (int)((recordCount) / INTERVAL_COUNT);

                MemoryStatistics[i] = new MemoryStatistics(INTERVAL_COUNT);
                MemoryStatistics[i].Step = (int)((recordCount) / INTERVAL_COUNT);

                IOStatistics[i] = new IOStatistics(INTERVAL_COUNT);
                IOStatistics[i].Step = (int)((recordCount) / INTERVAL_COUNT);
            }

            Cancellation = cancellation;
        }

        #region Test Methods

        public void Init()
        {
            StartTime = DateTime.Now;

            try
            {
                SpeedStatistics[(int)TestMethod.Write].Start();

                Database.Init(FlowCount, RecordCount);
            }
            finally
            {
                SpeedStatistics[(int)TestMethod.Write].Stop();
            }
        }

        /// <summary>
        /// Execute a write test into the database.
        /// </summary>
        public void Write()
        {
            CurrentMethod = TestMethod.Write;
            int method = (int)CurrentMethod;

            IEnumerable<KeyValuePair<long, Tick>>[] flows = new IEnumerable<KeyValuePair<long, Tick>>[FlowCount];
            for (int k = 0; k < flows.Length; k++)
                flows[k] = GetFlow();

            try
            {
                SpeedStatistics[method].Start();
                ProcessorStatistics[method].Start();
                MemoryStatistics[method].Start();
                IOStatistics[method].Start();

                Task[] tasks = DoWrite(flows);
                Task.WaitAll(tasks);
            }
            finally
            {
                CurrentMethod = TestMethod.None;

                SpeedStatistics[method].Stop();
                ProcessorStatistics[method].Stop();
                MemoryStatistics[method].Stop();
                IOStatistics[method].Stop();
            }
        }

        /// <summary>
        /// Execute a read test from the database.
        /// </summary>
        public void Read()
        {
            CurrentMethod = TestMethod.Read;
            int method = (int)CurrentMethod;

            try
            {
                SpeedStatistics[method].Start();
                ProcessorStatistics[method].Start();
                MemoryStatistics[(int)TestMethod.Read].Start();
                IOStatistics[(int)TestMethod.Read].Start();

                Task task = DoRead(TestMethod.Read);
                Task.WaitAll(task);
            }
            finally
            {
                CurrentMethod = TestMethod.None;

                SpeedStatistics[method].Stop();
                ProcessorStatistics[method].Stop();
                MemoryStatistics[method].Stop();
                IOStatistics[method].Stop();
            }
        }

        /// <summary>
        /// Execute a secondary read from the database.
        /// </summary>
        public void SecondaryRead()
        {
            CurrentMethod = TestMethod.SecondaryRead;
            int method = (int)CurrentMethod;

            try
            {
                SpeedStatistics[method].Start();
                ProcessorStatistics[method].Start();
                MemoryStatistics[method].Start();
                IOStatistics[method].Start();

                Task tasks = DoRead(TestMethod.SecondaryRead);
                Task.WaitAll(tasks);
            }
            finally
            {
                CurrentMethod = TestMethod.None;

                SpeedStatistics[method].Stop();
                ProcessorStatistics[method].Stop();
                MemoryStatistics[method].Stop();
                IOStatistics[method].Stop();
            }
        }

        public void Finish()
        {
            DatabaseSize = Database.Size;

            SpeedStatistics[(int)TestMethod.SecondaryRead].Start();

            try
            {
                Database.Finish();
                EndTime = DateTime.Now;
            }
            finally
            {
                SpeedStatistics[(int)TestMethod.SecondaryRead].Stop();
            }
        }

        #endregion

        #region Statistics

        public TimeSpan GetTime(TestMethod method)
        {
            lock (SpeedStatistics)
            {
                return SpeedStatistics[(int)method].Time;
            }
        }

        public double GetSpeed(TestMethod method)
        {
            lock (SpeedStatistics)
            {
                return SpeedStatistics[(int)method].Speed;
            }
        }

        public long GetRecords(TestMethod method)
        {
            lock (SpeedStatistics)
            {
                return SpeedStatistics[(int)method].Count;
            }
        }

        public float GetAverageProcessorTime(TestMethod method)
        {
            lock (ProcessorStatistics)
            {
                return ProcessorStatistics[(int)method].AverageProcessorTime;
            }
        }

        public float GetPeakWorkingSet(TestMethod method)
        {
            lock (MemoryStatistics)
            {
                return MemoryStatistics[(int)method].PeakWorkingSet;
            }
        }

        public float GetAverageIOData(TestMethod method)
        {
            lock (IOStatistics)
            {
                return IOStatistics[(int)method].AverageIOData;
            }
        }

        /// <summary>
        /// Get average speed of the current database in records/sec.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetAverageSpeed(TestMethod method, int position)
        {
            lock (SpeedStatistics)
            {
                var array = SpeedStatistics[(int)method].RecordTime;
                var count = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < count; position++)
                {
                    var records = array[position].Key;
                    var speed = (records / array[position].Value.TotalSeconds);

                    yield return new KeyValuePair<long, double>(records, speed);
                }
            }
        }

        /// <summary>
        /// Get moment speed of the current database in records/sec.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetMomentSpeed(TestMethod method, int position)
        {
            lock (SpeedStatistics)
            {
                var array = SpeedStatistics[(int)method].RecordTime;
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {
                    var records = array[position].Key;
                    var oldRecords = array[position - 1].Key;
                    var currentElapsed = array[position].Value.TotalSeconds;
                    var previousElapsed = array[position - 1].Value.TotalSeconds;

                    var speed = (records - oldRecords) / (currentElapsed - previousElapsed);

                    yield return new KeyValuePair<long, double>(records, speed);
                }
            }
        }

        public IEnumerable<KeyValuePair<long, float>> GetAverageUserTimeProcessor(TestMethod method, int position)
        {
            lock (ProcessorStatistics)
            {
                var array = ProcessorStatistics[(int)method].AverageUserTimeStats.ToArray();
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {

                    var records = array[position].Key;
                    var userTime = array[position].Value;

                    yield return new KeyValuePair<long, float>(records, userTime);
                }
            }
        }

        /// <summary>
        /// Gets the average memory working set in bytes.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> GetAverageWorkingSet(TestMethod method, int position)
        {
            lock (MemoryStatistics)
            {
                var array = MemoryStatistics[(int)method].AverageWorkingSetStats.ToArray();
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {
                    var records = array[position].Key;
                    var workingSet = array[position].Value;

                    yield return new KeyValuePair<long, float>(records, workingSet);
                }
            }
        }

        /// <summary>
        /// Gets the average process I/O.
        /// </summary>
        public IEnumerable<KeyValuePair<long, float>> GetAverageDataIO(TestMethod method, int position)
        {
            lock (IOStatistics)
            {
                var array = IOStatistics[(int)method].AverageDataIOStats.ToArray();
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {
                    var records = array[position].Key;
                    var io = array[position].Value;

                    yield return new KeyValuePair<long, float>(records, io);
                }
            }
        }

        #endregion

        private IEnumerable<KeyValuePair<long, Tick>> GetFlow()
        {
            Thread.Sleep(10); // TODO: Remove this workaround at some point.
            SemiRandomGenerator generator = new SemiRandomGenerator(Randomness);
            
            return TicksGenerator.GetFlow(RecordCount, generator);
        }

        #region Benchmark Methods

        /// <summary>
        /// Wraps a data flow to check cancellation token and accumulate some statistic.
        /// </summary>
        private IEnumerable<KeyValuePair<long, Tick>> Wrap(IEnumerable<KeyValuePair<long, Tick>> flow, CancellationToken token, params IStatistic[] statistics)
        {
            foreach (var kv in flow)
            {
                if (token.IsCancellationRequested)
                    yield break;

                yield return kv;

                for (int i = 0; i < statistics.Length; i++)
                {
                    lock(statistics)
                        statistics[i].Add();
                }
            }
        }

        private Task[] DoWrite(IEnumerable<KeyValuePair<long, Tick>>[] flows)
        {
            Task[] tasks = new Task[flows.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew((state) =>
                {
                    int index = (int)state;
                    int method = (int)TestMethod.Write;
                    var flow = Wrap(flows[index], Cancellation.Token, SpeedStatistics[method], ProcessorStatistics[method], MemoryStatistics[method], IOStatistics[method]);
                    
                    Database.Write(index, flow);

                }, i, Cancellation.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            return tasks;
        }

        private Task DoRead(TestMethod method)
        {
            Comparer<long> comparer = Comparer<long>.Default;

            Task task = Task.Factory.StartNew((state) =>
            {
                bool ordered = true;

                int methodIndex = (int)state;
                var flow = Wrap(Database.Read(), Cancellation.Token, SpeedStatistics[methodIndex], ProcessorStatistics[methodIndex], MemoryStatistics[methodIndex], IOStatistics[methodIndex]);

                long oldKey = 0;
                int counter = 0;

                foreach (var kv in flow)
                {
                    var key = kv.Key;
                    if (counter == 0)
                    {
                        oldKey = key;
                        counter++;

                        continue;
                    }

                    int cmp = comparer.Compare(oldKey, key);
                    if (cmp > 0)
                    {
                        ordered = false;
                        break;
                    }

                    oldKey = key;
                    counter++;
                }

                if (!ordered)
                    throw new Exception("Keys are not ordered.");

            }, (int)method, Cancellation.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return task;
        }

        #endregion

        /// <summary>
        /// Exports benchmark sessions to a *.csv file.
        /// </summary>
        public static void ExportSessions(List<BenchmarkTest> sessions, string fileName)
        {
            if (sessions.Count == 0)
                return;

            // ---Write Detailed Results File---

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                var tableCount = sessions[0].FlowCount;
                var recordCount = sessions[0].RecordCount;
                var sequential = sessions[0].KeysType.ToString();
                var randomness = string.Format("{0}",sessions[0].Randomness);

                // write settings
                writer.WriteLine("Settings:");
                writer.WriteLine(String.Join(";", Enumerable.Repeat("TableCount;RecordCount;KeysType;Randomness", 1)));
                writer.Write(tableCount + ";");
                writer.Write(recordCount + ";");
                writer.Write(sequential + ";");
                writer.WriteLine(randomness + ";");

                writer.WriteLine();

                // write databases
                writer.WriteLine(String.Join(";;;;;;;;;;", sessions.Select(x => x.Database.DatabaseName)));
                writer.WriteLine(String.Join(";", Enumerable.Repeat("Records;WriteTimeSpan;ReadTimeSpan;SecondaryReadTimeSpan;AverageWrite;AverageRead;AverageSecondaryRead;MomentWrite;MomentRead;MomentSecondaryRead", sessions.Count)));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < INTERVAL_COUNT + 1; i++)
                {
                    for (int k = 0; k < sessions.Count; k++)
                    {
                        BenchmarkTest session = sessions[k];

                        // get statistics
                        SpeedStatistics writeStat = session.SpeedStatistics[(int)TestMethod.Write];
                        SpeedStatistics readStat = session.SpeedStatistics[(int)TestMethod.Read];
                        SpeedStatistics secondaryReadStat = session.SpeedStatistics[(int)TestMethod.SecondaryRead];

                        // calculate average speeds
                        var avgWriteSpeed = writeStat.GetAverageSpeedAt(i);
                        var avgReadSpeed = readStat.GetAverageSpeedAt(i);
                        var avgSecondaryReadSpeed = secondaryReadStat.GetAverageSpeedAt(i);

                        // calculate moment speeds
                        var momentWriteSpeed = writeStat.GetMomentSpeedAt(i);
                        var momentReadSpeed = readStat.GetMomentSpeedAt(i);
                        var momentSecondaryReadSpeed = secondaryReadStat.GetMomentSpeedAt(i);

                        // number of records & write timespan
                        var rec = writeStat.GetRecordAt(i);
                        builder.AppendFormat("{0};{1};", rec.Key, rec.Value.TotalMilliseconds);

                        // read timespan
                        rec = readStat.GetRecordAt(i);
                        builder.AppendFormat("{0};", rec.Value.TotalMilliseconds);

                        // secondary read timespan
                        rec = secondaryReadStat.GetRecordAt(i);
                        builder.AppendFormat("{0};", rec.Value.TotalMilliseconds);

                        // speeds
                        builder.Append(avgWriteSpeed + ";");
                        builder.Append(avgReadSpeed + ";");
                        builder.Append(avgSecondaryReadSpeed + ";");
                        builder.Append(momentWriteSpeed + ";");
                        builder.Append(momentReadSpeed + ";");
                        builder.Append(momentSecondaryReadSpeed + ";");
                    }

                    writer.WriteLine(builder.ToString());
                    builder.Clear();
                }

                // write size
                writer.WriteLine();
                writer.WriteLine(String.Join(";;;;;;;;;;", Enumerable.Repeat("Size(MB)", sessions.Count)));
                writer.WriteLine(String.Join(";;;;;;;;;;", sessions.Select(x => x.Database.Size / (1024.0 * 1024.0))));
            }

            // ---Write Summary File---
            //#time
            //#database;write speed (rec/sec);read speed;secondary read speed;size (MB)

            string extension = ".csv";

            string[] fullPath = fileName.Split(new string[] { extension }, StringSplitOptions.None);
            string summaryFileName = fullPath[0] + ".summary" + extension;

            using (StreamWriter streamWriter = new StreamWriter(summaryFileName))
            {
                StringBuilder builder2 = new StringBuilder();

                var tableCount = sessions[0].FlowCount;
                var recordCount = sessions[0].RecordCount;
                var sequential = sessions[0].KeysType.ToString();
                var randomness = string.Format("{0}", sessions[0].Randomness);

                // write settings
                builder2.AppendLine("Settings:");
                builder2.AppendLine(String.Join(";", Enumerable.Repeat("TableCount;RecordCount;KeysType;Randomness", 1)));
                builder2.Append(tableCount + ";");
                builder2.Append(recordCount + ";");
                builder2.Append(sequential + ";");
                builder2.AppendLine(randomness + ";");

                builder2.AppendLine();

                // write date
                builder2.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH.mm"));
                builder2.AppendLine();

                builder2.AppendLine(String.Join(";", "Database;WriteSpeed(rec/sec);ReadSpeed(rec/sec);SeconadyReadSpeed(rec/sec);Size(MB)"));

                for (int i = 0; i < sessions.Count; i++)
                {
                    BenchmarkTest test = sessions[i];

                    // write database info
                    builder2.Append(test.Database.DatabaseName + ";");
                    builder2.Append(test.GetSpeed(TestMethod.Write) + ";");
                    builder2.Append(test.GetSpeed(TestMethod.Read) + ";");
                    builder2.Append(test.GetSpeed(TestMethod.SecondaryRead) + ";");
                    builder2.Append(test.DatabaseSize / (1024.0 * 1024.0) + ";");

                    streamWriter.WriteLine(builder2.ToString());

                    builder2.Clear();
                }
            }
        }
    }
}
