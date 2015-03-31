using DatabaseBenchmark.Statistics;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseBenchmark.Exceptions;

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

            // Statistics.
            int length = Enum.GetValues(typeof(TestMethod)).Length - 1;

            SpeedStatistics = new SpeedStatistics[length];
            ProcessorStatistics = new ProcessorStatistics[length];
            MemoryStatistics = new MemoryStatistics[length];
            IOStatistics = new IOStatistics[length];

            int step = (int)((recordCount) / INTERVAL_COUNT);

            for (int i = 0; i < length; i++)
            {
                SpeedStatistics[i] = new SpeedStatistics(INTERVAL_COUNT);
                SpeedStatistics[i].Step = step;

                //ProcessorStatistics[i] = new ProcessorStatistics(INTERVAL_COUNT);
                //ProcessorStatistics[i].Step = step;

                MemoryStatistics[i] = new MemoryStatistics(INTERVAL_COUNT);
                MemoryStatistics[i].Step = step;

                //IOStatistics[i] = new IOStatistics(INTERVAL_COUNT);
                //IOStatistics[i].Step = step;
            }

            Cancellation = cancellation;
        }

		private void StartStatistics(int method)
		{
			SpeedStatistics[method].Start();
			//ProcessorStatistics[method].Start();
			MemoryStatistics[method].Start();
			//IOStatistics[method].Start();
		}

		private void StopStatistics(int method)
		{
			SpeedStatistics[method].Stop();
			//ProcessorStatistics[method].Stop();
			MemoryStatistics[method].Stop();
			//IOStatistics[method].Stop();
		}

        #region Test Methods

        public void Init()
        {
            StartTime = DateTime.Now;

			int method = (int)TestMethod.Write;

			try
            {
				StartStatistics(method);

				Database.Init(FlowCount, RecordCount);
            }
            finally
            {
				StopStatistics(method);
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
				StartStatistics(method);

				Task[] tasks = DoWrite(flows);
                Task.WaitAll(tasks, Cancellation.Token);
            }
            finally
            {
                CurrentMethod = TestMethod.None;

				StopStatistics(method);
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
				StartStatistics(method);

				Task task = DoRead(TestMethod.Read);
                Task.WaitAll(new Task[] { task }, Cancellation.Token);
            }
            finally
            {
                CurrentMethod = TestMethod.None;

				StopStatistics(method);
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
				StartStatistics(method);

				Task task = DoRead(TestMethod.SecondaryRead);
                Task.WaitAll(new Task[] { task }, Cancellation.Token);
            }
            finally
            {
                CurrentMethod = TestMethod.None;

				StopStatistics(method);
			}
        }

        public void Finish()
        {
            DatabaseSize = Database.Size;

			int method = (int)TestMethod.SecondaryRead;

			StartStatistics(method);

            try
            {
                Database.Finish();
                EndTime = DateTime.Now;
            }
            finally
            {
				StopStatistics(method);
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
                return ProcessorStatistics[(int)method].MomentProcessorTime;
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
                return IOStatistics[(int)method].MomentIOData;
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

        public IEnumerable<KeyValuePair<long, double>> GetAverageUserTimeProcessor(TestMethod method, int position)
        {
            lock (ProcessorStatistics)
            {
                var array = ProcessorStatistics[(int)method].MomentUserTimeStats.ToArray();
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {

                    var records = array[position].Key;
                    var userTime = array[position].Value;

                    yield return new KeyValuePair<long, double>(records, userTime);
                }
            }
        }

        /// <summary>
        /// Gets the average memory working set in bytes.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetMomentWorkingSet(TestMethod method, int position)
        {
            lock (MemoryStatistics)
            {
                var array = MemoryStatistics[(int)method].MomentWorkingSetStats.ToArray();
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {
                    var records = array[position].Key;
                    var workingSet = array[position].Value;

                    yield return new KeyValuePair<long, double>(records, workingSet);
                }
            }
        }

        /// <summary>
        /// Gets the average process I/O.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetAverageDataIO(TestMethod method, int position)
        {
            lock (IOStatistics)
            {
                var array = IOStatistics[(int)method].MomentDataIOStats.ToArray();
                var length = array.Length;

                if (position == 0)
                    position = 1;

                for (; position < length; position++)
                {
                    var records = array[position].Key;
                    var io = array[position].Value;

                    yield return new KeyValuePair<long, double>(records, io);
                }
            }
        }

        #endregion

        private IEnumerable<KeyValuePair<long, Tick>> GetFlow()
        {
            // TODO: Remove this workaround at some point.
            Thread.Sleep(10); // Ensures different seed for the generators.
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
                    var flow = Wrap(flows[index], Cancellation.Token, SpeedStatistics[method], MemoryStatistics[method]);
                    
                    Database.Write(index, flow);

                }, i, Cancellation.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            return tasks;
        }

        private Task DoRead(TestMethod method)
        {
            Task task = Task.Factory.StartNew((state) =>
            {
                int methodIndex = (int)state;
                var flow = Wrap(Database.Read(), Cancellation.Token, SpeedStatistics[methodIndex], MemoryStatistics[methodIndex]);

                bool ordered = true;
                long previous = flow.First().Key;

                foreach (var kv in flow)
                {
                    var key = kv.Key;

                    if (previous > key)
                    {
                        ordered = false;
                        break;
                    }

                    previous = key;
                }

                if (!ordered)
                    throw new KeysNotOrderedException("Keys are not ordered.");

            }, (int)method, Cancellation.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return task;
        }

        #endregion
    }
}
