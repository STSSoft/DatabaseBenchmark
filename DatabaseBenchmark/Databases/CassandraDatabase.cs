using Cassandra;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using DatabaseBenchmark.Benchmarking;

namespace DatabaseBenchmark.Databases
{
    public class CassandraDatabase : Database
    {
        private const int TABLE_ID = 0;
        private const int QUERY_READ_LIMIT = 10000;

        private string KeySpace = "SpeedTest";
        private ICluster cluster;
        private ISession[] sessions;

        /// <summary>
        /// Specifies how many records are inserted with every batch.
        /// </summary>
        public int InsertsPerQuery { get; set; }

        public CassandraDatabase()
        {
            DatabaseName = "CassandraDB";
            DatabaseCollection = "table1";
            Category = "NoSQL\\Wide Column Store Column Families";
            Description = "CassandraDB 2.1.2 + CassandraToLinq";
            Website = "http://cassandra.apache.org/";
            Color = System.Drawing.Color.FromArgb(185, 229, 250);

            Requirements = new string[]
            {
                "Cassandra.dll",
                "Cassandra.Data.Linq.dll"
            };

            InsertsPerQuery = 1000;
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            ConnectionString = ConnectionString == null ? "localhost" : ConnectionString;

            sessions = new ISession[flowCount];

            cluster = Cluster.Builder().AddContactPoint(ConnectionString).Build();
            ISession session = cluster.Connect();

            session.DeleteKeyspaceIfExists(KeySpace);
            session.CreateKeyspace(KeySpace);
            session.ChangeKeyspace(KeySpace);

            sessions[0] = session;

            for (int i = 0; i < flowCount; i++)
            {
                session = cluster.Connect();
                session.ChangeKeyspace(KeySpace);

                sessions[i] = session;
            }

            sessions[0].Execute(GetCreateTableQuery(DatabaseCollection));

            if (ConnectionString == "localhost")
                DataDirectory = Path.Combine(GetConfigFilePath(), "data\\data");
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            ISession session = sessions[flowID];
            int insertCount = 0;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("BEGIN BATCH");

            foreach (var kv in flow)
            {
                var tick = kv.Value;

                builder.Append(" UPDATE ");
                builder.Append(DatabaseCollection);
                builder.Append(" SET Symbol = ");
                builder.Append("'");
                builder.Append(tick.Symbol);
                builder.Append("'");
                builder.Append(", ");
                builder.Append("Timestamp = ");
                builder.Append(tick.Timestamp.Ticks);
                builder.Append(", ");
                builder.Append("Bid = ");
                builder.Append(tick.Bid.ToString().Replace(',', '.'));
                builder.Append(", ");
                builder.Append("Ask = ");
                builder.Append(tick.Ask.ToString().Replace(',', '.'));
                builder.Append(", ");
                builder.Append("BidSize = ");
                builder.Append(tick.BidSize);
                builder.Append(", ");
                builder.Append("AskSize = ");
                builder.Append(tick.AskSize);
                builder.Append(", ");
                builder.Append("Provider = ");
                builder.Append("'");
                builder.Append(tick.Provider);
                builder.Append("' ");
                builder.Append("WHERE TableID = ");
                builder.Append(TABLE_ID);
                builder.Append(" AND Key = ");
                builder.Append(kv.Key);
                builder.Append(";");
                builder.Append(Environment.NewLine);

                insertCount++;

                if (insertCount == InsertsPerQuery)
                {
                    builder.AppendLine("APPLY BATCH");
                    string g = builder.ToString();
                    Console.WriteLine(g);
                    session.Execute(builder.ToString());
                    builder.Clear();
                    builder.AppendLine("BEGIN BATCH");
                    insertCount = 0;
                }

                /*Direct build of insert batches skipping linq layer for speed performance.

                 BEGIN BATCH
                 UPDATE table1 SET Symbol = 'BGNJPY', Timestamp = 635585735580459280, Bid = 68.36, Ask = 68.54, BidSize = 9768, AskSize = 4136, Provider = 'NASDAQ' WHERE TableID = 0 AND Key = 1128096811;
                 UPDATE table1 SET Symbol = 'EURGBP', Timestamp = 635585735770459280, Bid = 0.8437, Ask = 0.8451, BidSize = 93, AskSize = 5630, Provider = 'NYSE' WHERE TableID = 0 AND Key = 444946454;
                 UPDATE table1 SET Symbol = 'CHFUSD', Timestamp = 635585735920459280, Bid = 1.087, Ask = 1.0884, BidSize = 7002, AskSize = 1971, Provider = 'ASE' WHERE TableID = 0 AND Key = 888296084;
                 APPLY BATCH*/
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            long lastKey = long.MinValue;
            bool hasMoreRecords = true;

            //Paging records.
            while (hasMoreRecords)
            {
                hasMoreRecords = false;

                var queryStr = string.Format("SELECT * FROM {0} where Key > {2} limit {1} allow filtering;", DatabaseCollection, QUERY_READ_LIMIT, lastKey);
                var query = sessions[0].Execute(queryStr);
                foreach (var row in query.GetRows())
                {
                    hasMoreRecords = true;

                    Tick tick = new Tick();
                    lastKey = (long)row[1];
                    tick.Symbol = (string)row[7];
                    tick.Timestamp = new DateTime((long)row[8]);
                    tick.Bid = (double)row[2];
                    tick.Ask = (double)row[4];
                    tick.BidSize = (int)row[3];
                    tick.AskSize = (int)row[5];
                    tick.Provider = (string)row[6];

                    yield return new KeyValuePair<long, Tick>(lastKey, tick);
                }
            }
        }

        public override void Finish()
        {
            cluster.Shutdown();

            foreach (var session in sessions)
                session.Dispose();
        }

        private static string GetCreateTableQuery(string name)
        {
            return String.Format(@"CREATE TABLE {0} (
            TableID int, Key bigint, Symbol text, Timestamp bigint, Bid double, Ask double, BidSize int, AskSize int, Provider text, PRIMARY KEY (TableID, Key)) WITH CLUSTERING ORDER BY (Key ASC) ", name);
        }

        private static string GetConfigFilePath()
        {
            string wmiQuery = string.Format("select CommandLine from Win32_Process where Name='{0}'", "java.exe");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();

            string cmd = string.Empty;
            foreach (ManagementObject retObject in retObjectCollection)
            {
                var line = retObject["CommandLine"];
                cmd = line.ToString();
            }

            string conf = "\\conf";
            int pos = cmd.IndexOf(conf) + conf.Length - 1;

            string confPath = string.Empty;
            while (cmd[pos] != '\"')
                confPath += cmd[pos--];

            confPath = new string(confPath.Reverse().ToArray());

            return confPath.Remove(confPath.Length - 5);
        }

        public static string FindDataPath(string configPath)
        {
            var configFile = File.ReadAllText(Path.Combine(configPath, "cassandra.yaml"));

            string dataFileDirectories = "data_file_directories:";
            int indexData = configFile.IndexOf(dataFileDirectories) + dataFileDirectories.Length + 1;
            string dataDir = string.Empty;

            while (configFile[indexData] != '\n')
            {
                dataDir += configFile[indexData];
                indexData++;
            }

            dataDir = new string(dataDir.ToCharArray().Where(x => x != ' ').ToArray()).Substring(1);

            if (!dataDir.Contains(":"))
                dataDir = configPath.Substring(0, 2) + dataDir;

            return dataDir;
        }
    }
}
