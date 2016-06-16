using log4net;
using System;
using DatabaseBenchmark.Core.Exceptions;
using DatabaseBenchmark.Core.Properties;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DatabaseBenchmark.Core.Statistics;
using DatabaseBenchmark.Core.Reporting;

namespace DatabaseBenchmark.Core
{
    /// <summary>
    /// Represents a benchmark suite that executes tests for a specific database.
    /// </summary>
    public class Benchmark
    {
        public const int INTERVAL_COUNT = 100;

        private ILog Logger;

        /// <summary>
        /// Invoked when a performance report is started.
        /// </summary>
        public event Action<PerformanceWatch> OnStart;

        /// <summary>
        /// Invoked when a performance report is stopped.
        /// </summary>
        public event Action<PerformanceWatch> OnStop;

        /// <summary>
        /// The current database instance used for the tests.
        /// </summary>
        public IDatabase Database { get; private set; }

        /// <summary>
        /// The currently active test.
        /// </summary>
        public ITest CurrentTest { get; private set; }

        public Benchmark(IDatabase database)
        {
            Logger = LogManager.GetLogger(Settings.Default.TestLogger);

            Database = database;
        }

        public void ExecuteTests(CancellationToken token, params ITest[] tests)
        {
            if (token == CancellationToken.None)
                throw new ArgumentException("Cancellation token is empty.");

            if (tests == null)
                throw new ArgumentNullException("Tests array is null.");

            foreach (var test in tests)
            {
                if (test.Database != Database)
                    throw new ArgumentException(String.Format("Database {0} does not match internal database {1}", test.Database.Name, Database.Name));

                CurrentTest = test;

                try
                {
                    WireEvents();

                    CurrentTest.Start(token);
                }
                catch (Exception exc)
                {
                    Logger.Error("Test execution error...", exc);
                }
            }

            CurrentTest = null;
        }

        //public PerformanceReport GetReport()
        //{
        //    PerformanceReport report = new PerformanceReport();

        //    report.ElapsedTime = 
        //}

        /// <summary>
        /// Get moment speed entries of the current database in records/sec for the current running test.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetMomentSpeeds(int position)
        {
            if (CurrentTest == null || CurrentTest.ActiveReport == null)
                yield break;

            lock (CurrentTest.ActiveReport)
            {
                var array = CurrentTest.ActiveReport.SpeedStatistics.RecordTime;
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

        /// <summary>
        /// Get average speed entries of the current database in records/sec for the current running test.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetAverageSpeeds(int position)
        {
            if (CurrentTest == null || CurrentTest.ActiveReport == null)
                yield break;

            lock (CurrentTest.ActiveReport)
            {
                var array = CurrentTest.ActiveReport.SpeedStatistics.RecordTime;
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
        /// Gets  average memory working set entries in bytes for the current running test.
        /// </summary>
        public IEnumerable<KeyValuePair<long, double>> GetMomentWorkingSets(int position)
        {
            if (CurrentTest == null || CurrentTest.ActiveReport == null)
                yield break;

            lock (CurrentTest.ActiveReport)
            {
                var array = CurrentTest.ActiveReport.MemoryStatistics.MomentWorkingSetStats.ToArray();
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

        private void LogOnStart(PerformanceWatch report)
        {
            Logger.Info(String.Format("{0} started.", report.Name));
        }

        private void LogOnStop(PerformanceWatch report)
        {
            Logger.Info(String.Format("{0} stopped.", report.Name));
        }

        private void WireEvents()
        {
            if (CurrentTest.ActiveReport == null)
                return;

            CurrentTest.ActiveReport.OnStart += OnStart;
            CurrentTest.ActiveReport.OnStart += LogOnStart;

            CurrentTest.ActiveReport.OnStop += OnStop;
            CurrentTest.ActiveReport.OnStop += LogOnStop;
        }
    }
}
