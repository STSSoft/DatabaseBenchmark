using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Json;

namespace DatabaseBenchmark.Report
{
    public static class JsonUtils
    {
        /// <summary>
        /// Exports the given data into a JSON file.
        /// </summary>
        public static void ExportToJson(string path, ComputerConfiguration configuration, List<BenchmarkTest> benchmarks, ReportType type)
        {
            List<JsonObjectCollection> collection = new List<JsonObjectCollection>();

            collection.Add(ConvertToJson(configuration));

            foreach (var benchmark in benchmarks)
                collection.Add(ConvertToJson(benchmark, type));

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                StreamWriter writer = new StreamWriter(stream);

                JsonObjectCollection json = new JsonObjectCollection(collection);
                json.WriteTo(writer);

                writer.Flush();
            }
        }

        /// <summary>
        /// Convert the data into a JSON format.
        /// </summary>
        public static JsonObjectCollection ConvertToJson(UserInfo user, ComputerConfiguration configuration, List<BenchmarkTest> benchmarks)
        {
            List<JsonObjectCollection> jsonData = new List<JsonObjectCollection>();

            // User.
            var jsonUser = ConvertToJson(user);
            jsonData.Add(jsonUser);

            // Computer configuration.
            var jsonComputer = ConvertToJson(configuration);
            jsonData.Add(jsonComputer);
            
            // Benchmark test.
            foreach (var benchmark in benchmarks)
            {
                var jsonTest = ConvertToJson(benchmark, ReportType.Detailed);
                jsonData.Add(jsonTest);
            }

            return new JsonObjectCollection(jsonData);
        }

        public static JsonObjectCollection ConvertToJson(UserInfo user)
        {
            JsonObjectCollection jsonUser = new JsonObjectCollection("User");
            jsonUser.Add(new JsonStringValue("Email", user.Email));
            jsonUser.Add(new JsonStringValue("AdditionalInfo", user.AdditionalInfo));

            return jsonUser;
        }

        public static JsonObjectCollection ConvertToJson(BenchmarkTest benchmark, ReportType type)
        {
            JsonObjectCollection jsonBenchmark = new JsonObjectCollection("BenchmarkTest");

            // Test info parameters.
            JsonObjectCollection jsonSettings = new JsonObjectCollection("TestInfo");
            jsonSettings.Add(new JsonNumericValue("FlowCount", benchmark.FlowCount));
            jsonSettings.Add(new JsonNumericValue("RecordCount", benchmark.RecordCount));
            jsonSettings.Add(new JsonNumericValue("Randomness", benchmark.Randomness * 100));

            long elapsedTime = benchmark.EndTime.Ticks - benchmark.StartTime.Ticks;
            jsonSettings.Add(new JsonNumericValue("ElapsedTime", new TimeSpan(elapsedTime).TotalMilliseconds));

            JsonObjectCollection jsonDatabase = new JsonObjectCollection("Database");
            jsonDatabase.Add(new JsonStringValue("Name", benchmark.Database.Name));
            jsonDatabase.Add(new JsonStringValue("IndexingTechnology", benchmark.Database.IndexingTechnology.ToString()));
            jsonDatabase.Add(new JsonStringValue("Category", benchmark.Database.Name));
            jsonDatabase.Add(new JsonNumericValue("Size", benchmark.DatabaseSize / (1024.0 * 1024.0)));

            if (benchmark.Database.Settings != null)
            {
                JsonObjectCollection jsonDatabaseSettings = new JsonObjectCollection("Settings");

                foreach (var item in benchmark.Database.Settings)
                    jsonDatabaseSettings.Add(new JsonStringValue(item.Key, item.Value));

                jsonDatabase.Add(jsonDatabaseSettings);
            }

            //jsonDatabase.Add(new JsonStringValue("Settings", ReflectionUtils.GetPublicPropertyValues(benchmark.Database))); // TODO: sync with remote server

            // Test results.
            JsonObjectCollection jsonTestData = new JsonObjectCollection("TestResults");
            JsonObject jsonWrite = null;
            JsonObject jsonRead = null;
            JsonObject jsonSecondaryRead = null;

            if (type == ReportType.Summary)
            {
                jsonWrite = new JsonNumericValue("Write", benchmark.GetSpeed(TestMethod.Write));
                jsonRead = new JsonNumericValue("Read", benchmark.GetSpeed(TestMethod.Read));
                jsonSecondaryRead = new JsonNumericValue("SecondaryRead", benchmark.GetSpeed(TestMethod.SecondaryRead));
            }
            else // type == ReportType.Detailed
            {
                // Get statistics and convert them to JSON.
                SpeedStatistics writeStat = benchmark.SpeedStatistics[(int)TestMethod.Write];
                SpeedStatistics readStat = benchmark.SpeedStatistics[(int)TestMethod.Read];
                SpeedStatistics secondaryReadStat = benchmark.SpeedStatistics[(int)TestMethod.SecondaryRead];

                jsonWrite = ConvertStatisticToJson(writeStat, "Write");
                jsonRead = ConvertStatisticToJson(readStat, "Read");
                jsonSecondaryRead = ConvertStatisticToJson(secondaryReadStat, "SecondaryRead");
            }

            jsonTestData.Add(jsonWrite);
            jsonTestData.Add(jsonRead);
            jsonTestData.Add(jsonSecondaryRead);

            // Form the end JSON structure.
            jsonBenchmark.Add(jsonSettings);
            jsonBenchmark.Add(jsonDatabase);
            jsonBenchmark.Add(jsonTestData);

            return jsonBenchmark;
        }

        #region Statistics to JSON

        public static JsonObjectCollection ConvertStatisticToJson(SpeedStatistics statistic, string statisticName)
        {
            JsonObjectCollection jsonTest = new JsonObjectCollection(statisticName);
            JsonArrayCollection jsonRecords = new JsonArrayCollection("Records");
            JsonArrayCollection jsonTime = new JsonArrayCollection("Time");
            JsonArrayCollection jsonAverageSpeed = new JsonArrayCollection("AverageSpeed");
            JsonArrayCollection jsonMomentSpeed = new JsonArrayCollection("MomentSpeed");

            for (int i = 0; i < BenchmarkTest.INTERVAL_COUNT; i++)
            {
                // Number of records & timespan.
                var rec = statistic.GetRecordAt(i);
                jsonRecords.Add(new JsonNumericValue(rec.Key));
                jsonTime.Add(new JsonNumericValue(rec.Value.TotalMilliseconds));
               
                // Average speed.
                var averageSpeed = statistic.GetAverageSpeedAt(i);
                jsonAverageSpeed.Add(new JsonNumericValue(averageSpeed));


                // Moment write speed.
                var momentSpeed = statistic.GetMomentSpeedAt(i);
                jsonMomentSpeed.Add(new JsonNumericValue(momentSpeed));
            }

            jsonTest.Add(jsonRecords);
            jsonTest.Add(jsonTime);
            jsonTest.Add(jsonAverageSpeed);
            jsonTest.Add(jsonMomentSpeed);

            return jsonTest;
        }

        #endregion

        #region Computer configuration to JSON

        public static JsonObjectCollection ConvertToJson(ComputerConfiguration configuration)
        {
            var jsonOS = ConvertToJson(configuration.OperatingSystem);
            var jsonProcessors = ConvertToJson(configuration.Processors);
            var jsonMemoryModules = ConvertToJson(configuration.MemoryModules);
            var jsonStorageDevices = ConvertToJson(configuration.StorageDevices);

            JsonObjectCollection jsonConfiguration = new JsonObjectCollection("ComputerConfiguration");

            jsonConfiguration.Add(jsonOS);
            jsonConfiguration.Add(jsonProcessors);
            jsonConfiguration.Add(jsonMemoryModules);
            jsonConfiguration.Add(jsonStorageDevices);

            return jsonConfiguration;
        }

        public static JsonObjectCollection ConvertToJson(OperatingSystemInfo operatingSystem)
        {
            JsonObjectCollection jsonOS = new JsonObjectCollection("OperatingSystem");
            jsonOS.Add(new JsonStringValue("Name", operatingSystem.Name));
            jsonOS.Add(new JsonBooleanValue("Is64Bit", operatingSystem.Is64bit));

            return jsonOS;
        }

        public static JsonArrayCollection ConvertToJson(List<CpuInfo> processors)
        {
            JsonArrayCollection jsonProcessors = new JsonArrayCollection("Processors");

            int index = 0;
            foreach (var processor in processors)
            {
                JsonObjectCollection jsonCPU = new JsonObjectCollection();

                jsonCPU.Add(new JsonStringValue("Name", processor.Name));
                jsonCPU.Add(new JsonNumericValue("Threads", processor.Threads));
                jsonCPU.Add(new JsonNumericValue("MaxClockSpeed", processor.MaxClockSpeed));

                jsonProcessors.Add(jsonCPU);
                index++;
            }

            return jsonProcessors;
        }

        public static JsonArrayCollection ConvertToJson(List<RamInfo> memoryModules)
        {
            JsonArrayCollection jsonMemoryModules = new JsonArrayCollection("MemoryModules");

            int index = 0;
            foreach (var module in memoryModules)
            {
                JsonObjectCollection jsonMemoryModule = new JsonObjectCollection();

                jsonMemoryModule.Add(new JsonStringValue("Type", module.MemoryType.ToString()));
                jsonMemoryModule.Add(new JsonNumericValue("Capacity", module.Capacity));
                jsonMemoryModule.Add(new JsonNumericValue("Speed", module.Speed));

                jsonMemoryModules.Add(jsonMemoryModule);
                index++;
            }

            return jsonMemoryModules;
        }

        public static JsonArrayCollection ConvertToJson(List<StorageDeviceInfo> storageDrives)
        {
            JsonArrayCollection jsonStorages = new JsonArrayCollection("Storages");

            int index = 0;
            foreach (var storage in storageDrives)
            {
                JsonObjectCollection storageDevice = new JsonObjectCollection();

                storageDevice.Add(new JsonStringValue("Model", storage.Model));
                storageDevice.Add(new JsonNumericValue("Capacity", storage.Size));

                jsonStorages.Add(storageDevice);
                index++;
            }

            return jsonStorages;
        }

        #endregion
    }  
}
