using STS.General.Extensions;
using STS.General.SQL.Extensions;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Databases
{
    public class MSSQLServerDatabase : Database
    {
        private IDbConnection[] connections;
        private IDbCommand[] commands;

        public override IndexingTechnology IndexingTechnology
        {
            get { return IndexingTechnology.BTree; }
        }

        public MSSQLServerDatabase()
        {   
            Name = "MSSQL Server 2012";
            CollectionName = "table1";
            Category = "SQL";
            Description = "Microsoft SQL Server 2012 (SP1) - 11.0.3128.0 (X64) Dec 28 2012";
            Website = "https://www.microsoft.com/en-us/sqlserver/default.aspx";
            Color = System.Drawing.Color.FromArgb(207, 91, 95);

            Requirements = new string[] 
            { 
                "System.Data.dll",
                "System.Data.Entity.dll"
            };

#if DEBUG
            ConnectionString = "Server=SFT3STOYCHEV;Database=MSSQLTest;Uid=test1;Pwd=123456789;";
#else
            ConnectionString = "Server=ETR3SVETOSLAV;Database=Test;Uid=test7;Pwd=123456;";
#endif
        }

        private IDbConnection GetConnection()
        {
            IDbConnection conn = new SqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        private string CreateTableQuery(string tableName)
        {
            return string.Format("CREATE TABLE {0} (", tableName) +
                      "[ID] BIGINT PRIMARY KEY WITH(IGNORE_DUP_KEY = ON), " +
                      "[Symbol] varchar(255), " +
                      "[Time] DateTime, " +
                      "[Bid] REAL, " +
                      "[Ask] REAL, " +
                      "[BidSize] int, " +
                      "[AskSize] int, " +
                      "[Provider] varchar(255))";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            connections = new IDbConnection[flowCount];
            commands = new IDbCommand[flowCount];

            for (int i = 0; i < flowCount; i++)
            {
                var connection = GetConnection();
                connections[i] = connection;

                commands[i] = CreateCommand(connection);
            }

            connections[0].ExecuteNonQuery(String.Format("IF OBJECT_ID('{0}', 'U') IS NOT NULL DROP TABLE {0};", CollectionName));
            connections[0].ExecuteNonQuery(CreateTableQuery(CollectionName));
            connections[0].ExecuteNonQuery(String.Format(@"IF OBJECT_ID('{0}', 'P') IS NOT NULL DROP PROC {0}", CollectionName));
            connections[0].ExecuteNonQuery(String.Format(@"CREATE PROCEDURE dbo.InsertOrUdpateTest 
                   @ID bigint, @symbol VARCHAR(255), @time DateTime, @bid real, @ask real, @bidSize int, @askSize int, @provider varchar(255)
                        AS BEGIN
                        	BEGIN TRAN
                        
                            IF NOT EXISTS (SELECT * FROM {0} WHERE ID = @ID)
                               INSERT INTO {0} VALUES(@ID, @symbol, @time, @bid, @ask, @bidSize, @askSize, @provider)
                            ELSE
                               UPDATE {0}
                               SET Symbol = @symbol, [Time] = @time, Bid = @bid,Ask = @ask, BidSize = @bidSize, AskSize = @askSize, Provider = @provider
                               WHERE ID = @ID
                        
                        	COMMIT TRAN
                        END;", CollectionName));
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            var command = commands[flowID];

            foreach (var item in flow)
            {
                var rec = item.Value;

                ((IDbDataParameter)command.Parameters[0]).Value = item.Key;
                ((IDbDataParameter)command.Parameters[1]).Value = rec.Symbol;
                ((IDbDataParameter)command.Parameters[2]).Value = rec.Timestamp;
                ((IDbDataParameter)command.Parameters[3]).Value = rec.Bid;
                ((IDbDataParameter)command.Parameters[4]).Value = rec.Ask;
                ((IDbDataParameter)command.Parameters[5]).Value = rec.BidSize;
                ((IDbDataParameter)command.Parameters[6]).Value = rec.AskSize;
                ((IDbDataParameter)command.Parameters[7]).Value = rec.Provider;

                command.ExecuteNonQuery();
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            IDataReader reader = connections[0].ExecuteQuery(string.Format("SELECT * FROM {0} ORDER BY {1}", CollectionName, "ID"));

            foreach (var row in reader.Forward())
            {
                long key = row.GetInt64(0);

                Tick tick = new Tick();
                tick.Symbol = row.GetString(1);
                tick.Timestamp = row.GetDateTime(2);
                tick.Bid = row.GetFloat(3);
                tick.Ask = row.GetFloat(4);
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
                    long size = 0;

                    for (int i = 0; i < connections.Length; i++)
                    {
                        IDbCommand comand = conn.CreateCommand(String.Format("sp_spaceused '{0}'", CollectionName));

                        IDataReader reader = comand.ExecuteReader();
                        while (reader.Read())
                            size += long.Parse(reader["data"].ToString().Split(' ')[0]) * 1024;

                        reader.Close();
                    }

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
                return null;
            }
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

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "InsertOrUdpateTest";

            return command;
        }
    }
}
