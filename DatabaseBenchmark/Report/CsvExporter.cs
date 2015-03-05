using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Statistics;

namespace DatabaseBenchmark.Report
{
    public class CsvExporter
    {
        /// <summary>
        /// Exports benchmark sessions to a *.csv file.
        /// </summary>
        public static void ExportTestResults(List<BenchmarkTest> sessions, string path)
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
