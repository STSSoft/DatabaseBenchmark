using MySql.Data.MySqlClient;
using STS.General.Generators;
using STS.General.SQL;
using STS.General.SQL.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Databases
{
    public enum MySQLStorageEngine
    {
        MyISAM,
        INNODB,
        TokuDB,
        MEMORY,
        ARCHIVE,
        BLACKHOLE
    }

    public class MySQLDatabase : Database
    {
        private IDbConnection[] connections;
        private SQLMultiInsert[] helpers;

        [Category("Settings")]
        public int InsertsPerQuery { get; set; } 
        public MySQLStorageEngine StorageEngine { get; set; }

        protected MySQLDatabase(MySQLStorageEngine engine)
        {
            StorageEngine = engine;

            Name = String.Format("MySQL ({0})", StorageEngine);
            Category = "SQL";
            Description = "MySQL + .NET Connector";
            Website = "http://www.mysql.com/";
            Color = System.Drawing.Color.FromArgb(215, 233, 247);

            Requirements = new string[]
            { 
                "MySql.Data.dll"
            };

            MySqlConnectionStringBuilder cb = new MySqlConnectionStringBuilder();
            cb.ConnectionLifeTime = 0;
            cb.DefaultCommandTimeout = 0;
            cb.Server = "localhost";
            cb.UserID = "root";
            cb.Password = "";
            cb.Database = "test";

            CollectionName = "table1";
            ConnectionString = cb.ConnectionString;

            InsertsPerQuery = 1000;
        }

        protected IDbConnection GetConnection()
        {
            IDbConnection conn = new MySqlConnection();
            conn.Open();

            return conn;
        }

        protected virtual string CreateTableQuery()
        {
            return String.Format("CREATE TABLE `{0}` (", CollectionName) +
                      "`ID` bigint(20) NOT NULL," +
                      "`Symbol` varchar(255) NOT NULL," +
                      "`Time` datetime NOT NULL," + 
                      "`Bid` double NOT NULL," +
                      "`Ask` double NOT NULL," +
                      "`BidSize` int(20) NOT NULL," +
                      "`AskSize` int(20) NOT NULL," +
                      "`Provider` varchar(255) NOT NULL," +
                      "PRIMARY KEY (`ID`)" +
                    String.Format(") ENGINE={0};", StorageEngine);
        }

        protected virtual SQLMultiInsert GetInsertHelper(IDbConnection conn)
        {
            SQLMultiInsert helper = new SQLMultiInsert(conn, CollectionName, InsertsPerQuery);
            helper.InsertCommand = "replace"; // the old row is deleted before the new row is inserted

            helper.AddField("ID", DbType.Int64, 0);
            helper.AddField("Symbol", DbType.String, 255);
            helper.AddField("Time", DbType.DateTime, 0);
            helper.AddField("Bid", DbType.Double, 0);
            helper.AddField("Ask", DbType.Double, 0);
            helper.AddField("BidSize", DbType.Int32, 0);
            helper.AddField("AskSize", DbType.Int32, 0);
            helper.AddField("Provider", DbType.String, 255);

            helper.Prepare();

            return helper;
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            connections = new IDbConnection[flowCount];
            helpers = new SQLMultiInsert[flowCount];

            IDbConnection connection = null;
            for (int i = 0; i < flowCount; i++)
            {
                connection = GetConnection();
                connections[i] = connection;

                helpers[i] = GetInsertHelper(connection);
            }

            connections.First().ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS `{0}`;", CollectionName));
            connections.First().ExecuteNonQuery(CreateTableQuery());
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            var helper = helpers[flowID];

            foreach (var kv in flow)
            {
                var key = kv.Key;
                var rec = kv.Value;

                helper.Insert(key, rec.Symbol, rec.Timestamp, rec.Bid, rec.Ask, rec.BidSize, rec.AskSize, rec.Provider);
            }

            helper.Flush();
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            IDataReader reader = connections.First().ExecuteQuery(String.Format("SELECT * FROM {0} ORDER BY {1};", CollectionName, "ID"));

            foreach (var row in reader.Forward())
            {
                long key = row.GetInt64(0);

                Tick tick = new Tick();
                tick.Symbol = row.GetString(1);
                tick.Timestamp = row.GetDateTime(2);
                tick.Bid = row.GetDouble(3);
                tick.Ask = row.GetDouble(4);
                tick.BidSize = row.GetInt32(5);
                tick.AskSize = row.GetInt32(6);
                tick.Provider = row.GetString(7);

                yield return new KeyValuePair<long, Tick>(key, tick);
            }
        }

        public override void Finish()
        {
            for (int i = 0; i < connections.Length; i++)
                connections[i].Close();
        }

        public override long Size
        {
            get
            {
                IDbConnection conn = GetConnection();

                try
                {
                    string tables = String.Join(" OR ", Enumerable.Range(0, connections.Length).Select(x => String.Format("table_name = '{0}'", CollectionName)));
                    string query = "";

                    // Special query for TokuDB engine size. 
                    if (StorageEngine == MySQLStorageEngine.TokuDB)
                        query = String.Format("select sum(bt_size_allocated) from information_schema.TokuDB_fractal_tree_info where table_schema='{0}' and ({1});", conn.Database, tables);
                    else
                        query = String.Format("SELECT SUM(Data_length + Index_length) FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = '{0}' and ({1});", conn.Database, tables);

                    IDataReader reader = conn.ExecuteQuery(query);

                    long size = 0;
                    if (reader.Read())
                        size = reader.GetInt64(0);

                    reader.Close();

                    return size;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        [XmlIgnore]
        public override Dictionary<string, string> Settings
        {
            get
            {
                var settings = new Dictionary<string, string>();

                settings.Add("InsertsPerQuery", InsertsPerQuery.ToString());

                return settings;
            }
        }
    }
}
