using Scimore.Data.ScimoreClient;
using STS.General.Generators;
using STS.General.SQL.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Databases
{
    public class ScimoreDBDatabase : Database//Return 1 record less and slower with prepared statement
    {
        private ScimoreEmbedded scimore;

        private IDbConnection[] connections;
        private IDbCommand[] commands;

        public override IndexingTechnology IndexingTechnology
        {
            get { return IndexingTechnology.BTree; }
        }

        public ScimoreDBDatabase()
        {
            SyncRoot = new object();

            Name = "ScimoreDB";
            CollectionName = "table1";
            Category = "SQL";
            Description = "ScimoreDB 4.0 + ADO.NET Data Provider";
            Website = "http://www.scimore.com/";
            Color = System.Drawing.Color.FromArgb(234, 190, 52);

            Requirements = new string[] 
            { 
                "Scimore.Data.ScimoreClient.dll",
                "Scimore.Data.ScimoreClientNative.dll",
                "Scimore.Data.ScimoreEmbeddedDBNative.dll"
            };
        }

        private IDbConnection GetConnection()
        {
            IDbConnection conn = scimore.CreateConnection();
            conn.Open();

            return conn;
        }

        private string CreateTableQuery(string tableName)
        {
            //"`Time` datetime NOT NULL," + //because MySQL discard milliseconds
            return String.Format(@"CREATE TABLE {0}
                       ([ID] BIGINT NOT NULL PRIMARY KEY,
                        [Symbol] varchar(255) NOT NULL,
                        [Time] BIGINT  NOT NULL, 
                        [Bid] double NOT NULL,
                        [Ask] double NOT NULL,
                        [BidSize] BIGINT  NOT NULL,
                        [AskSize] BIGINT  NOT NULL,
                        [Provider] varchar(255) NOT NULL);", tableName);
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            scimore = new ScimoreEmbedded();
            scimore.MaxConnections = flowCount;

            string dataBaseName = "test";
            string dbInstanceName = String.Format(DataDirectory, "Database.scimore");

            scimore.Create(dbInstanceName);
            scimore.OpenInProcess(dbInstanceName);

            connections = new IDbConnection[flowCount];
            commands = new IDbCommand[flowCount];

            IDbConnection connection = GetConnection();

            connection.ExecuteNonQuery(String.Format("CREATE DATABASE {0}", dataBaseName));
            connection.ChangeDatabase(dataBaseName);
            connection.ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS {0};", CollectionName));
            connection.ExecuteNonQuery(CreateTableQuery(CollectionName));

            connections[0] = connection;
            commands[0] = CreateCommand(connection);

            for (int i = 1; i < flowCount; i++)
            {
                connection = GetConnection();
                connection.ChangeDatabase(dataBaseName);

                connections[i] = GetConnection();
                commands[i] = CreateCommand(connection);
            }
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            var command = commands[flowID];

            lock (SyncRoot)
            {
                foreach (var kv in flow)
                {
                    var key = kv.Key;
                    var rec = kv.Value;

                    ((IDbDataParameter)command.Parameters[0]).Value = key;
                    ((IDbDataParameter)command.Parameters[1]).Value = rec.Symbol;
                    ((IDbDataParameter)command.Parameters[2]).Value = rec.Timestamp.Ticks;
                    ((IDbDataParameter)command.Parameters[3]).Value = rec.Bid;
                    ((IDbDataParameter)command.Parameters[4]).Value = rec.Ask;
                    ((IDbDataParameter)command.Parameters[5]).Value = rec.BidSize;
                    ((IDbDataParameter)command.Parameters[6]).Value = rec.AskSize;
                    ((IDbDataParameter)command.Parameters[7]).Value = rec.Provider;

                    command.CommandText = String.Format("SELECT * FROM {0} WHERE ID = @ID", CollectionName);

                    IDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                        command.CommandText = String.Format(@"UPDATE {0} SET Symbol = @symbol, [Time] = @time, Bid = @bid, Ask = @ask, BidSize = @bidSize, AskSize = @askSize, Provider = @provider WHERE ID = @ID", CollectionName);
                    else
                        command.CommandText = String.Format("INSERT INTO {0} VALUES(@ID, @symbol, @time, @bid, @ask, @bidSize, @askSize, @provider);", CollectionName);

                    reader.Close();
                    command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            var cmd = new ScimoreCommand("SELECT  *  FROM " + CollectionName + " [ORDER BY {ID} [ASC]];", (ScimoreConnection)connections[0]);

            foreach (var row in cmd.ExecuteReader().Forward())
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
        }

        public override void Finish()
        {
            foreach (var connection in connections)
                connection.Close();
        }

        [XmlIgnore]
        public override Dictionary<string, string> Settings
        {
            get
            {
                return null;
            }
        }

        private IDbCommand CreateCommand(IDbConnection connection)
        {
            var command = connection.CreateCommand();
            IDbDataParameter key = command.CreateParameter("@ID", DbType.Int64, 0);
            IDbDataParameter symbol = command.CreateParameter("@symbol", DbType.String, 255);
            IDbDataParameter time = command.CreateParameter("@time", DbType.Int64, 0);
            IDbDataParameter bid = command.CreateParameter("@bid", DbType.Double, 0);
            IDbDataParameter ask = command.CreateParameter("@ask", DbType.Double, 0);
            IDbDataParameter bidSize = command.CreateParameter("@bidSize", DbType.Int32, 0);
            IDbDataParameter askSize = command.CreateParameter("@askSize", DbType.Int32, 0);
            IDbDataParameter provider = command.CreateParameter("@provider", DbType.String, 255);

            command.Parameters.Add(key);
            command.Parameters.Add(symbol);
            command.Parameters.Add(time);
            command.Parameters.Add(bid);
            command.Parameters.Add(ask);
            command.Parameters.Add(bidSize);
            command.Parameters.Add(askSize);
            command.Parameters.Add(provider);

            command.CommandType = CommandType.Text;

            return command;
        }
    }
}
