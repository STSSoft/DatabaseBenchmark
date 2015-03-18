using Npgsql;
using STS.General.Extensions;
using STS.General.Generators;
using STS.General.SQL;
using System;
using System.Collections.Generic;
using System.Data;

namespace DatabaseBenchmark.Databases
{
    public class PostgreSQLDatabase : Database
    {
        private IDbConnection[] connections;

        public PostgreSQLDatabase()
        {    
            DatabaseName = "Postgre";
            DatabaseCollection = "table1";
            Category = "SQL";
            Description = "PostgreSQL + Npgsql 2.0.13.91 .NET Data Provider";
            Website = "http://www.postgresql.org/";
            Color = System.Drawing.Color.FromArgb(0, 148, 196);

            Requirements = new string[]
            { 
                "MySql.Data.dll" 
            };

            NpgsqlConnectionStringBuilder cs = new NpgsqlConnectionStringBuilder();
            cs.Host = "localhost";
            cs.Port = 5432;
            cs.UserName = "postgres";
            cs.Password = "123456789";
            cs.CommandTimeout = 0;
            cs.ConnectionLifeTime = 0;
            ConnectionString = cs.ToString();
        }

        private IDbConnection GetConnection()
        {
            IDbConnection conn = null;

            conn = new NpgsqlConnection(ConnectionString);
            conn.Open();

            return conn;
        }

        private string CreateTableQuery(string tableName)
        {
            return "CREATE TABLE " + tableName + "(" +
                "ID bigint NOT NULL," +
                "Symbol text," +
                "Time timestamp without time zone," +
                "Bid double precision," +
                "Ask double precision," +
                "BidSize integer," +
                "AskSize integer," +
                "Provider text," +
                "PRIMARY KEY (ID)" +
                ");";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            connections = new IDbConnection[flowCount];

            for (int i = 0; i < flowCount; i++)
            {
                IDbConnection connection = GetConnection();
                connections[i] = connection;
            }

            connections[0].ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS {0};", DatabaseCollection));
            connections[0].ExecuteNonQuery(CreateTableQuery(DatabaseCollection));
            connections[0].ExecuteNonQuery(string.Format(@"
            CREATE OR REPLACE FUNCTION InsertToTable1(_id bigint, _symbol text, _time timestamp without time zone, _bid double precision, _ask double precision, _bidsize int, _asksize int, _provider text ) RETURNS int LANGUAGE plpgsql AS $$
            DECLARE
            	rowCount RECORD;
            BEGIN
            
              IF exists(select * from {0} where id=_id) THEN
            	UPDATE {0} SET symbol =_symbol, time = _time, bid = _bid, ask = _ask, bidsize = _bidsize, asksize =_asksize, provider = _provider WHERE ID = _id;
              ELSE
            	INSERT INTO {0} VALUES(_id,_symbol,_time,_bid, _ask, _bidsize, _asksize, _provider);
              END IF;

              COMIT;
              
              RETURN 0;
            END
            $$;", DatabaseCollection));
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var connection = connections[flowID];

            foreach (var kv in flow)
            {
                var key = kv.Key;
                var rec = kv.Value;

                connection.ExecuteNonQuery(string.Format(@"
                            SELECT InsertToTable1(CAST({0} as bigint),
		                    CAST('{1}' as text), 
		                    CAST('{2}' as timestamp without time zone), 
		                    CAST('{3}' as double precision),
		                    CAST('{4}' as double precision),
		                    CAST({5} as int),
		                    CAST({6} as int),
		                    CAST('{7}' as text));",
                        key, rec.Symbol, rec.Timestamp.ToString(), rec.Bid.ToString(),
                        rec.Ask.ToString(), rec.BidSize, rec.AskSize, rec.Provider));
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
            foreach (var connection in connections)
                connection.Close();
        }

        public override long Size
        {
            get
            {
                IDbConnection conn = GetConnection();

                try
                {
                    string query = String.Format("SELECT pg_total_relation_size('postgres.public.{0}')", DatabaseCollection);

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
    }
}
