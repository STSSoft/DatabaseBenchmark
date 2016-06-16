using DatabaseBenchmark.Core.Benchmarking;
using DatabaseBenchmark.Core.Utils;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CommandLine;
using CommandLine.Text;
using DatabaseBenchmark.Report;

namespace DatabaseBenchmark
{
    public class Program
    {
        public const string DEFAULT_CONFIGURATION_FILE = "DefaultConfig.config";

        private static Task MainTask = null;
        private static Timer Timer = null;

        private static CancellationTokenSource Cancellation;

        private static BenchmarkSession CurrentSession = null;
        private static TestMethod CurrentMethod = TestMethod.None;
        private static string CurrentStatus = String.Empty;

        private static long RecordsCount = 0;
        private static List<BenchmarkSession> History = new List<BenchmarkSession>();

        private static ILog Logger = LogManager.GetLogger(Settings.Default.Logger);                                             

        static void Main(string[] args)
        {
            try
            {
                var result = Parser.Default.ParseArguments<ConsoleOptions>(args)
                    .WithParsed<ConsoleOptions>(RunBenchmark)
                    .WithNotParsed<ConsoleOptions>(HandleParsingErrors);
            }
            catch (Exception exc)
            {
                Logger.Error("Application error...", exc);
            }
        }

        public static void HandleParsingErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
                Logger.Error(error);
        }

        public static void RunBenchmark(ConsoleOptions options)
        {
            FileStream configFile = null;

            if (options.ConfigFilePath != null)
                configFile = new FileStream(options.ConfigFilePath, FileMode.Open);

            if (options.CreateConfigFile != null)
            {
                ConfigurationFactory.CreateConfigFile(options.CreateConfigFile);
                Logger.Info(String.Format("{0} created succesfully.", options.CreateConfigFile));

                return;
            }

            if (File.Exists(DEFAULT_CONFIGURATION_FILE))
                configFile = new FileStream(DEFAULT_CONFIGURATION_FILE, FileMode.Open);
            else
            {
                Logger.Error("Default configuration file not found. Please use '--createConfig' command.");
                return;
            }

            Console.WriteLine();
            Logger.Info("Parsing configuration file...");

            var configuration = ConfigurationFactory.ParseConfigFile(configFile);
            RecordsCount = configuration.FlowCount * configuration.RecordCount;

            Logger.Info(String.Format("------------------Test parameters------------------"));
            Logger.Info(String.Format("Flow count: {0}", configuration.FlowCount));
            Logger.Info(String.Format("Records Count: {0}", configuration.RecordCount));
            Logger.Info(String.Format("Randomness: {0}", configuration.Randomness));

            Console.WriteLine();
            Logger.Info(String.Format("------------------Included databases------------------"));

            foreach (var database in configuration.Databases)
                Logger.Info(database.Name);

            Logger.Info(String.Format("------------------------------------------------------"));

            // Start the tests.
            Start(configuration);

            Timer = new Timer(UpdateConsoleTitle);
            Timer.Change(0, 700);

            Task.WaitAll(MainTask);

            //ExportResults(options);
        }

        public static void ExportResults(ConsoleOptions options)
        {
            ReportType type = ReportType.Summary;

            if(options.ReportType == "summary")
                type = ReportType.Summary;
            else if(options.ReportType == "detailed")
                type = ReportType.Detailed;

            if (options.CsvPath != null)
            {
                CsvUtils.ExportResults(History, options.CsvPath, type);
                Logger.Info("Results successfuly exported to CSV.");
            }

            if(options.JsonPath != null)
            {
                var configuration = SystemUtils.GetComputerConfiguration();

                JsonUtils.ExportToJson(options.JsonPath, configuration, History, type);
                Logger.Info("Results successfuly exported to JSON.");
            }
        }

        public static void Start(TestConfiguration configuration)
        {
            History.Clear();
            Cancellation = new CancellationTokenSource();

            foreach (var database in configuration.Databases)
            {
                var session = new BenchmarkSession(database, configuration.FlowCount, configuration.RecordCount, configuration.Randomness, Cancellation);
                History.Add(session);

                var directory = Path.Combine(configuration.DataDirectory, database.Name);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                else
                {
                    Directory.Delete(directory, true);
                    Directory.CreateDirectory(directory);
                }

                database.DataDirectory = directory;         
            }

            // Start the benchmark.
            MainTask = Task.Factory.StartNew(DoBenchmark, Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private static void DoBenchmark()
        {
            BenchmarkSuite testSuite = new BenchmarkSuite();
            testSuite.OnTestMethodCompleted += Report;

            try
            {
                foreach (var session in History)
                {
                    if (Cancellation.IsCancellationRequested)
                        break;

                    CurrentSession = session;

                    CurrentStatus = String.Format("{0} Init...", session.Database.Name);
                    testSuite.ExecuteInit(session);

                    CurrentStatus = String.Format("{0} Write...", session.Database.Name);
                    CurrentMethod = TestMethod.Write;
                    testSuite.ExecuteWrite(session);

                    CurrentStatus = String.Format("{0} Read...", session.Database.Name);
                    CurrentMethod = TestMethod.Read;
                    testSuite.ExecuteRead(session);

                    CurrentStatus = String.Format("{0} Secondary Read...", session.Database.Name);
                    CurrentMethod = TestMethod.SecondaryRead;
                    testSuite.ExecuteSecondaryRead(session);

                    testSuite.ExecuteFinish(session);
                }
            }
            finally
            {
                Console.Title = "Tests finished.";

                if (Cancellation.IsCancellationRequested)
                    History.Clear();
            }
        }

        private static void Report(BenchmarkSession benchmark, TestMethod method)
        {
            try
            {
                string databaseName = benchmark.Database.Name;

                Console.WriteLine();

                var speed = benchmark.GetAverageSpeed(method);
                var size = (benchmark.DatabaseSize / (1024.0 * 1024.0));
                var elapsedTime = new TimeSpan(benchmark.GetElapsedTime(method).Ticks);
                var peakWorkingSet = (benchmark.GetPeakWorkingSet(method) / (1024.0 * 1024.0));

                Logger.Info(String.Format("------------------{0} {1}------------------", databaseName, method.ToString()));
                Logger.Info(String.Format("Average speed: {0:#,#} rec/sec", speed));
                Logger.Info(String.Format("Size: {0:f1} MB", size));
                Logger.Info(String.Format("Elapsed time: {0}", elapsedTime));
                Logger.Info(String.Format("Peak memory usage: {0:f0} MB", peakWorkingSet));
                Logger.Info("---------------------------------------");

                Console.WriteLine();
            }
            catch (Exception exc)
            {
                Logger.Error("Report results failed...", exc);
            }
        }

        private static void UpdateConsoleTitle(object state)
        {
            if (CurrentMethod != TestMethod.None)
            {
                long currentRecords = CurrentSession.GetRecords(CurrentMethod);

                var progress = String.Format("{0:f2} %", (100.0 * currentRecords) / RecordsCount);
                Console.Title = CurrentStatus + progress;
            }
        }
    }
}
