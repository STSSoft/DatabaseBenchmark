using STS.General.Extensions;
using STS.General.Generators;
using STS.General.SQL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace DatabaseBenchmark.Databases
{
    public class SQLiteDatabase : Database
    {
        private const int MAX_RECORDS_PER_QUERY = 120; //The maximum number is 120

        private IDbConnection[] connections;
        private SQLMultiInsert[] helpers;
        private int insertsPerQuery;

        [Category("Connection Settings")]
        public bool InMemoryDatabase { get; set; }

        [Category("Connection Settings")]
        public int PageSize { get; set; }

        [Category("Connection Settings")]
        public int CacheSize { get; set; }

        /// <summary>
        /// Specifies how many records are inserted with every batch.
        /// The maximum number is 120
        /// </summary>
        [Category("Settings")]
        public int InsertsPerQuery
        {
            get { return insertsPerQuery; }
            set
            {
                insertsPerQuery = value;

                if (insertsPerQuery > MAX_RECORDS_PER_QUERY)
                    insertsPerQuery = MAX_RECORDS_PER_QUERY;
            }
        }

        public SQLiteDatabase()
        {
            SyncRoot = new object();

            Name = "SQLite";
            CollectionName = "table1";
            Category = "SQL";
            Description = "SQLite 3.8.4.1 + ADO.NET Provider for SQLite 1.0.92.0";
            Website = "http://www.sqlite.org/";
            Color = System.Drawing.Color.Aquamarine;

            Requirements = new string[]
            { 
                "SQLite.Interop",
                "System.Data.SQLite.dll"
            };

            PageSize = 1 * 1024 * 1024;
            CacheSize = 32768;
            InsertsPerQuery = MAX_RECORDS_PER_QUERY;
        }

        public string GetDefaultConnectionString()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = InMemoryDatabase ? ":memory:" : Path.Combine(DataDirectory, "database.sqlite3");
            builder.PageSize = PageSize;
            builder.DefaultTimeout = 0;
            builder.CacheSize = CacheSize;
            builder.JournalMode = SQLiteJournalModeEnum.Off;

            return builder.ConnectionString;
        }

        private SQLiteConnection GetConnection()
        {
            SQLiteConnection conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            conn.ExecuteNonQuery("PRAGMA synchronous = OFF;");
            conn.ExecuteNonQuery("PRAGMA count_changes = OFF");

            return conn;
        }

        private SQLMultiInsert GetInsertHelper(IDbConnection conn, string tableName)
        {
            SQLMultiInsert helper = new SQLMultiInsert(conn, tableName, MAX_RECORDS_PER_QUERY);
            helper.InsertCommand = "Insert or replace";

            helper.AddField("ID", DbType.Int64, 0);
            helper.AddField("Symbol", DbType.String, 255);
            helper.AddField("Time", DbType.Int64, 0);
            helper.AddField("Bid", DbType.Double, 0);
            helper.AddField("Ask", DbType.Double, 0);
            helper.AddField("BidSize", DbType.Int32, 0);
            helper.AddField("AskSize", DbType.Int32, 0);
            helper.AddField("Provider", DbType.String, 255);

            helper.Prepare();

            return helper;
        }

        private string CreateTableQuery(string tableName)
        {
            return String.Format("CREATE TABLE {0} (", tableName) +
                      "ID bigint," +
                      "Symbol varchar(255)," +
                      "Time bigint," +
                      "Bid double," +
                      "Ask double," +
                      "BidSize int," +
                      "AskSize int," +
                      "Provider varchar(255)," +
                      "PRIMARY KEY (ID));";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            ConnectionString = GetDefaultConnectionString();
            connections = new IDbConnection[flowCount];

            helpers = new SQLMultiInsert[flowCount];

            for (int i = 0; i < flowCount; i++)
            {
                var connection = GetConnection();
                connections[i] = connection;

                helpers[i] = GetInsertHelper(connection, CollectionName);
            }

            connections[0].ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS `{0}`;", CollectionName));
            connections[0].ExecuteNonQuery(CreateTableQuery(CollectionName));
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            var connection = connections[flowID];
            var helper = helpers[flowID];

            lock (SyncRoot)
            {
                foreach (var kv in flow)
                {
                    var key = kv.Key;
                    var rec = kv.Value;

                    helper.Insert(key, rec.Symbol, rec.Timestamp.Ticks, rec.Bid, rec.Ask, rec.BidSize, rec.AskSize, rec.Provider);
                }

                helper.Flush();
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            IDataReader reader = connections[0].ExecuteQuery(String.Format("SELECT * FROM {0} ORDER BY {1};", CollectionName, "ID"));

            foreach (var row in reader.Forward())
            {
                long key = row.GetInt64(0);

                Tick tick = new Tick();
                tick.Symbol = row.GetString(1);
                tick.Timestamp = new DateTime(row.GetInt64(2));
                tick.Bid = row.GetDouble(3);
                tick.Ask = row.GetDouble(4);
                tick.BidSize = row.GetInt32(5);
                tick.AskSize = row.GetInt32(6);
                tick.Provider = row.GetString(7);

                yield return new KeyValuePair<long, Tick>(key, tick);
            }

            reader.Close();
        }

        public override void Finish()
        {
            for (int i = 0; i < helpers.Length; i++)
            {
                connections[i].Close();
                helpers[i].Close();
            }
        }
    }
}