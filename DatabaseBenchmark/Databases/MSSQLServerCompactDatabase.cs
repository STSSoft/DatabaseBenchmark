using STS.General.Extensions;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;

namespace DatabaseBenchmark.Databases
{
    public class MSSQLServerCompactDatabase : Database
    {
        private IDbConnection[] connections;
        private IDbCommand[] commands;

        private string DatabaseFile
        {
            get { return Path.Combine(DataDirectory, "test.sdf"); }
        }

        public MSSQLServerCompactDatabase()
        {
            SyncRoot = new object();

            DatabaseName = "MSSQL Server Compact 4.0";
            DatabaseCollection = "table1";
            Category = "SQL";
            Description = "MSSQL Server Compact 4.0 x64";
            Website = "http://www.microsoft.com/en-us/download/details.aspx?id=17876";
            Color = System.Drawing.Color.Violet;

            Requirements = new string[]
            { 
                "sqlceca40.dll",
                "sqlcecompact40.dll",
                "sqlceer40EN.dll", 
                "sqlceme40.dll",
                "sqlceoledb40.dll",
                "sqlceqp40.dll",
                "sqlcese40.dll",
                "System.Data.SqlServerCe.dll"
            };

            DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, DatabaseName);
            ConnectionString = String.Format("Data Source={0};encryption mode=platform default;Password=123;", DatabaseFile);
        }

        private IDbConnection GetConnection()
        {
            IDbConnection conn = new SqlCeConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        private string CreateTableQuery(string tableName)
        {
            return String.Format("CREATE TABLE {0} (", tableName) +
                      "ID bigint NOT NULL PRIMARY KEY," +
                      "Symbol nvarchar (255) NOT NULL," +
                      "Time datetime NOT NULL," +
                      "Bid float NOT NULL," +
                      "Ask float NOT NULL," +
                      "BidSize int NOT NULL," +
                      "AskSize int NOT NULL," +
                      "Provider nvarchar (255) NOT NULL" +
                      ");";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            SqlCeEngine engine = new SqlCeEngine(ConnectionString);

            if (File.Exists(DatabaseFile))
                File.Delete(DatabaseFile);
            engine.CreateDatabase();

            connections = new IDbConnection[flowCount];
            commands = new IDbCommand[flowCount];

            for (int i = 0; i < flowCount; i++)
            {
                IDbConnection connection = GetConnection();
                connections[i] = connection;
                commands[i] = CreateCommand(connection);
            }

            connections[0].ExecuteNonQuery(CreateTableQuery(DatabaseCollection));
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            var command = commands[flowID];

            foreach (var kv in flow)
            {
                var key = kv.Key;
                var rec = kv.Value;

                lock (SyncRoot)
                {
                    ((IDbDataParameter)command.Parameters[0]).Value = key;
                    ((IDbDataParameter)command.Parameters[1]).Value = rec.Symbol;
                    ((IDbDataParameter)command.Parameters[2]).Value = rec.Timestamp;
                    ((IDbDataParameter)command.Parameters[3]).Value = rec.Bid;
                    ((IDbDataParameter)command.Parameters[4]).Value = rec.Ask;
                    ((IDbDataParameter)command.Parameters[5]).Value = rec.BidSize;
                    ((IDbDataParameter)command.Parameters[6]).Value = rec.AskSize;
                    ((IDbDataParameter)command.Parameters[7]).Value = rec.Provider;

                    command.CommandText = String.Format("SELECT * FROM {0} WHERE ID = @ID", DatabaseCollection);

                    if (!command.ExecuteReader().Read())
                        command.CommandText = string.Format("INSERT INTO {0} VALUES(@ID, @symbol, @time, @bid, @ask, @bidSize, @askSize, @provider)", DatabaseCollection);
                    else
                        command.CommandText = string.Format(@"UPDATE {0}
                               SET Symbol = @symbol, [Time] = @time, Bid = @bid,Ask = @ask, BidSize = @bidSize, AskSize = @askSize, Provider = @provider
                               WHERE ID = @ID", DatabaseCollection);

                    command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            IDataReader reader = connections[0].ExecuteQuery(String.Format("SELECT * FROM {0} ORDER BY {1};", DatabaseCollection, "ID"));

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

        private IDbCommand CreateCommand(IDbConnection connection)
        {
            var command = connection.CreateCommand();
            IDbDataParameter key = command.CreateParameter("@ID", DbType.Int64, 8);
            IDbDataParameter symbol = command.CreateParameter("@symbol", DbType.String, 255);
            IDbDataParameter time = command.CreateParameter("@time", DbType.DateTime, 255);
            IDbDataParameter bid = command.CreateParameter("@bid", DbType.Double, 8);
            IDbDataParameter ask = command.CreateParameter("@ask", DbType.Double, 8);
            IDbDataParameter bidSize = command.CreateParameter("@bidSize", DbType.Int32, 8);
            IDbDataParameter askSize = command.CreateParameter("@askSize", DbType.Int32, 8);
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
