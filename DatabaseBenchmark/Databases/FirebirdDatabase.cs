using FirebirdSql.Data.FirebirdClient;
using STS.General.Extensions;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace DatabaseBenchmark.Databases
{
    public class FirebirdDatabase : Database
    {
        //Contexts of Relation/Procedure/Views maximum allowed is 255 and limit the string is 64kB
        private const int MAX_INSERT_PER_QUERY = 80;

        private InsertHelper[] helpers;
        private FbConnection[] connections;

        private int insertsPerQuery;

        /// <summary>
        /// Specifies how many records are inserted with every batch.
        /// Maximum value is 80. 
        /// </summary>
        public int InsertsPerQuery
        {
            get { return insertsPerQuery; }
            set
            {
                insertsPerQuery = value;

                if (insertsPerQuery > MAX_INSERT_PER_QUERY)
                    insertsPerQuery = MAX_INSERT_PER_QUERY;
            }
        }

        public FirebirdDatabase()
        {
            Name = "Firebird";
            CollectionName = "table1";
            Category = "SQL";
            Description = "Firebird + ADO.NET Data Provider 3.2.0.0";
            Website = "http://www.firebirdsql.org/en/start/";
            Color = Color.Yellow;

            Requirements = new string[]
            { 
                "FirebirdSql.Data.FirebirdClient.dll"
            };

            insertsPerQuery = 80;
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            helpers = new InsertHelper[flowCount];
            connections = new FbConnection[flowCount];

            FbConnection.CreateDatabase(GetConnectionString());

            for (int i = 0; i < flowCount; i++)
            {
                connections[i] = new FbConnection(GetConnectionString());
                connections[i].Open();

                helpers[i] = new InsertHelper(insertsPerQuery, connections[i], CollectionName);
            }

            CreateTable(connections[0], CollectionName);
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            try
            {
                var helper = helpers[flowID];

                foreach (var kv in flow)
                    helper.Insert(kv.Key, kv.Value.Symbol, kv.Value.Timestamp, kv.Value.Bid, kv.Value.Ask, kv.Value.BidSize, kv.Value.BidSize, kv.Value.Provider);

                helpers[flowID].Flush();
            }
            catch (Exception) { }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            var connection = connections[0];
            var cmd = new FbCommand(String.Format("SELECT * FROM {0} ORDER BY ID ASC;", CollectionName), connection);

            IDataReader reader = cmd.ExecuteReader();

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
            foreach (var connection in connections)
                connection.Close();
        }

        public string GetConnectionString()
        {
            FbConnectionStringBuilder cb = new FbConnectionStringBuilder();
            cb.ServerType = FbServerType.Embedded;
            cb.Database = Path.Combine(DataDirectory, "Database.fdb");
            cb.UserID = "root";
            cb.Password = "";
            cb.Pooling = true;
            cb.MinPoolSize = 0;
            cb.MaxPoolSize = 50;
            cb.PacketSize = 8192;
            cb.Dialect = 3;
            cb.FetchSize = 10000;
            cb.ConnectionLifeTime = 15;

            return cb.ToString();
        }

        private void CreateTable(FbConnection fbConnection, string tableName)
        {
            string createTable = String.Format("CREATE TABLE {0}" +
                "(ID BIGINT NOT NULL PRIMARY KEY," +
                "Symbol VARCHAR(255) NOT NULL," +
                "TickTime TIMESTAMP NOT NULL," + //Time,Timestamp
                "Bid REAL NOT NULL," +
                "Ask REAL NOT NULL," +
                "BidSize INT NOT NULL," +
                "AskSize INT NOT NULL," +
                "Provider VARCHAR(255) NOT NULL" +
                ");"
                , tableName);

            fbConnection.ExecuteNonQuery(createTable);
        }

        private class InsertHelper
        {
            private string commandString;
            private int counterOfQueries;

            public int NumberOfQueries { get; private set; }
            public FbConnection Connection { get; private set; }
            public string TableName { get; private set; }

            public InsertHelper(int numberOfQueries, FbConnection connetion, string tableName)
            {
                NumberOfQueries = numberOfQueries;
                Connection = connetion;
                TableName = tableName;
            }

            public void Insert(long p1, string p2, DateTime dateTime, double p3, double p4, int p5, int p6, string p7)
            {
                if (counterOfQueries == 0)
                    commandString = "EXECUTE BLOCK AS BEGIN ";

                counterOfQueries++;

                var time = dateTime.ToString("yyyy/MM/dd HH:mm:ss");
                commandString += String.Format("UPDATE OR INSERT INTO {0}  (ID, Symbol, TickTime, Bid, Ask, BidSize, AskSize, Provider)"
                                                + "VALUES({1}, '{2}', '{3}', {4}, {5}, {6}, {7}, '{8}'); ",
                                                TableName,
                                                p1, p2,
                                                time, p3.ToString(CultureInfo.CreateSpecificCulture("en-GB")),
                                                p4.ToString(CultureInfo.CreateSpecificCulture("en-GB")), p5,
                                                p6, p7);

                if (counterOfQueries == NumberOfQueries)
                {
                    Flush();
                    counterOfQueries = 0;

                    return;
                }
            }

            public void Flush()
            {
                if (counterOfQueries == 0)
                    return;

                Connection.ExecuteNonQuery(commandString + " END");
            }
        }
    }
}