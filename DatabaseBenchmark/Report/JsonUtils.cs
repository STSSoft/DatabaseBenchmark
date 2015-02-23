using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Statistics;

namespace DatabaseBenchmark.Validation
{
    public static class JsonUtils
    {
        public static void ExportToJson(string fileName, ComputerConfiguration configuration, List<BenchmarkTest> benchmarks)
        {
            var jsonData = ConvertToJson(configuration, benchmarks);

            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                StreamWriter writer = new StreamWriter(stream);
                jsonData.WriteTo(writer);

                writer.Flush();
            }
        }

        public static JsonObjectCollection ConvertToJson(UserInfo user, ComputerConfiguration configuration, List<BenchmarkTest> benchmarks)
        {
            List<JsonObjectCollection> json = new List<JsonObjectCollection>();

            // User.
            var jsonUser = ConvertToJson(user);
            json.Add(jsonUser);

            // Computer configuration.
            var jsonComputer = ConvertToJson(configuration);
            json.Add(jsonComputer);
            
            // Benchmark test.
            foreach (var benchmark in benchmarks)
            {
                var jsonTest = ConvertToJson(benchmark);
                json.Add(jsonTest);
            }

            return new JsonObjectCollection(json);
        }

        public static JsonObjectCollection ConvertToJson(ComputerConfiguration configuration, List<BenchmarkTest> benchmarks)
        {
            List<JsonObjectCollection> json = new List<JsonObjectCollection>();

            // Computer configuration.
            var jsonComputer = ConvertToJson(configuration);
            json.Add(jsonComputer);

            // Benchmark test.
            foreach (var benchmark in benchmarks)
            {
                var jsonTest = ConvertToJson(benchmark);
                json.Add(jsonTest);
            }

            return new JsonObjectCollection(json);
        }

        public static JsonObjectCollection ConvertToJson(UserInfo user)
        {
            JsonObjectCollection jsonUser = new JsonObjectCollection("User");
            jsonUser.Add(new JsonStringValue("Email", user.Email));
            jsonUser.Add(new JsonStringValue("AdditionalInfo", user.AdditionalInfo));

            return jsonUser;
        }

        public static JsonObjectCollection ConvertToJson(BenchmarkTest benchmark)
        {
            JsonObjectCollection jsonBenchmark = new JsonObjectCollection("BenchmarkTest");

            // Write test parameters.
            JsonObjectCollection jsonSettings = new JsonObjectCollection("TestInfo");
            jsonSettings.Add(new JsonNumericValue("FlowCount", benchmark.FlowCount));
            jsonSettings.Add(new JsonNumericValue("RecordCount", benchmark.FlowCount));
            jsonSettings.Add(new JsonNumericValue("Randomness", benchmark.Randomness));

            long elapsedTime = benchmark.EndTime.Ticks - benchmark.StartTime.Ticks;
            jsonSettings.Add(new JsonNumericValue("ElapsedTime", elapsedTime));
            jsonSettings.Add(new JsonStringValue("DatabaseName", benchmark.Database.DatabaseName));
            jsonSettings.Add(new JsonNumericValue("DatabaseSize", benchmark.DatabaseSize / (1024.0 * 1024.0)));

            // Write test data.
            JsonObjectCollection jsonTestData = new JsonObjectCollection("TestResults");

            // Get statistics and convert them to JSON.
            SpeedStatistics writeStat = benchmark.SpeedStatistics[(int)TestMethod.Write];
            SpeedStatistics readStat = benchmark.SpeedStatistics[(int)TestMethod.Read];
            SpeedStatistics secondaryReadStat = benchmark.SpeedStatistics[(int)TestMethod.SecondaryRead];

            var jsonWrite = ConvertStatisticToJson(writeStat, "Write");
            var jsonRead = ConvertStatisticToJson(writeStat, "Read");
            var jsonSecondaryRead = ConvertStatisticToJson(writeStat, "SecondaryRead");

            // Form the end JSON structure.
            jsonTestData.Add(jsonWrite);
            jsonTestData.Add(jsonRead);
            jsonTestData.Add(jsonSecondaryRead);
          
            jsonBenchmark.Add(jsonSettings);
            jsonBenchmark.Add(jsonTestData);

            return jsonBenchmark;
        }

        public static JsonObjectCollection ConvertStatisticToJson(SpeedStatistics statistic, string statisticName)
        {
            JsonObjectCollection jsonTest = new JsonObjectCollection(statisticName);
            JsonArrayCollection jsonRecords = new JsonArrayCollection("Records");
            JsonArrayCollection jsonTime = new JsonArrayCollection("Time");
            JsonArrayCollection jsonAverageSpeed = new JsonArrayCollection("AverageSpeed");
            JsonArrayCollection jsonMomentSpeed = new JsonArrayCollection("MomentSpeed");

            for (int i = 0; i < BenchmarkTest.INTERVAL_COUNT + 1; i++)
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
