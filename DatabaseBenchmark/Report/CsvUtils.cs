using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Statistics;
using DatabaseBenchmark.Validation;

namespace DatabaseBenchmark.Report
{
    public class CsvUtils
    {
        /// <summary>
        /// Exports benchmark sessions to a *.csv file.
        /// </summary>
        public static void ExportSummaryTestResults(List<BenchmarkTest> sessions, string path)
        {
            // ---Write Summary File---
            //#time
            //#database;write speed (rec/sec);read speed;secondary read speed;size (MB)

            string extension = ".csv";

            string[] fullPath = path.Split(new string[] { extension }, StringSplitOptions.None);
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
                builder2.AppendLine(String.Join(";", Enumerable.Repeat("Table count;Record count;Keys type;Randomness", 1)));
                builder2.Append(tableCount + ";");
                builder2.Append(recordCount + ";");
                builder2.Append(sequential + ";");
                builder2.AppendLine(randomness + ";");

                builder2.AppendLine();

                // write date
                builder2.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH.mm"));
                builder2.AppendLine();

                builder2.AppendLine(String.Join(";", "Database;Write speed(rec/sec);Read speed(rec/sec);Seconady read speed(rec/sec);Size(MB)"));

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

                ExportConputerConfiguration(streamWriter, SystemUtils.GetComputerConfiguration());
            }
        }

        /// <summary>
        /// Exports benchmark sessions to a *.csv file.
        /// </summary>
        public static void ExportDetailedTestResults(List<BenchmarkTest> sessions, string path)
        {
            if (sessions.Count == 0)
                return;

            // ---Write Detailed Results File---

            using (StreamWriter writer = new StreamWriter(path))
            {
                var tableCount = sessions[0].FlowCount;
                var recordCount = sessions[0].RecordCount;
                var sequential = sessions[0].KeysType.ToString();
                var randomness = string.Format("{0}", sessions[0].Randomness);

                // write settings
                writer.WriteLine("Settings:");
                writer.WriteLine("Table count;Record count;Keys type;Randomness");
                writer.Write(tableCount + ";");
                writer.Write(recordCount + ";");
                writer.Write(sequential + ";");
                writer.WriteLine(randomness + ";");

                writer.WriteLine();

                // write databases
                writer.WriteLine(String.Join(";;;;;;;;;;;", sessions.Select(x => x.Database.DatabaseName)));
                writer.WriteLine(String.Join(";", Enumerable.Repeat("Records;Write time;Read time;Secondary read time;Average write;Average read;Average secondary read;Moment write;Moment read;Moment secondary read;", sessions.Count)));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < BenchmarkTest.INTERVAL_COUNT + 1; i++)
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
                        builder.Append(";");
                    }

                    writer.WriteLine(builder.ToString());
                    builder.Clear();
                }

                // write size
                writer.WriteLine();
                writer.WriteLine(String.Join(";;;;;;;;;;;", Enumerable.Repeat("Size(MB)", sessions.Count)));
                writer.WriteLine(String.Join(";;;;;;;;;;;", sessions.Select(x => x.Database.Size / (1024.0 * 1024.0))));
                writer.WriteLine();

                ExportConputerConfiguration(writer, SystemUtils.GetComputerConfiguration());
            }
        }

        private static void ExportConputerConfiguration(StreamWriter writer, ComputerConfiguration computerInfo)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Computer specification:");
            builder.AppendLine("Operating system;;;Processors;;;;Memory modules;;;;Storages");
            builder.AppendLine("Name;Is64Bit;;Name;Threads;Max clock speed (kHz);;Type;Capacity (GB);Speed (kHz);;Model;Size (GB);Partitions;");

            builder.Append(computerInfo.OperatingSystem.Name + ";");
            builder.Append(computerInfo.OperatingSystem.Is64bit + ";;");

            CpuInfo firstProcessor = computerInfo.Processors.First();
            builder.Append(firstProcessor.Name + ";");
            builder.Append(firstProcessor.Threads + ";");
            builder.Append(firstProcessor.MaxClockSpeed + ";;");

            RamInfo firstRam = computerInfo.MemoryModules.First();
            builder.Append(firstRam.MemoryType + ";");
            builder.Append(firstRam.Capacity + ";");
            builder.Append(firstRam.Speed + ";;");

            StorageDeviceInfo firstStorage = computerInfo.StorageDevices.First();
            builder.Append(firstStorage.Model + ";");
            builder.Append(firstStorage.Size + ";");
            firstStorage.DriveLetters.ForEach(x => builder.Append(x.Replace(":", "")));

            builder.Append(Environment.NewLine);

            int count = Math.Max(Math.Max(computerInfo.Processors.Count, computerInfo.MemoryModules.Count), computerInfo.StorageDevices.Count);

            for (int i = 1; i < count; i++)
            {
                builder.Append(";;;");

                if (computerInfo.Processors.Count <= i)
                    builder.Append(";;;;");
                else
                {
                    CpuInfo info = computerInfo.Processors[i];

                    builder.Append(info.Name + ";");
                    builder.Append(info.Threads + ";");
                    builder.Append(info.MaxClockSpeed + ";;");
                }

                if (computerInfo.MemoryModules.Count <= i)
                    builder.Append(";;;;");
                else
                {
                    RamInfo info = computerInfo.MemoryModules[i];

                    builder.Append(firstRam.MemoryType + ";");
                    builder.Append(firstRam.Capacity + ";");
                    builder.Append(firstRam.Speed + ";;");
                }

                if (computerInfo.StorageDevices.Count <= i)
                    builder.Append(";;;;");
                else
                {
                    StorageDeviceInfo info = computerInfo.StorageDevices[i];
                    builder.Append(info.Model + ";");
                    builder.Append(info.Size + ";");
                    info.DriveLetters.ForEach(x => builder.Append(x));
                }
            }

            writer.WriteLine(builder.ToString());
        }
    }
}
