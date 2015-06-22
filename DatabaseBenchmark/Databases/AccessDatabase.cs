using log4net;
using Microsoft.Office.Interop.Access.Dao;
using STS.General.Generators;
using STS.General.SQL.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Databases
{
    public class AccessDatabase : Database
    {
        private ILog Logger;
        private IDbConnection[] connections;
        private IDbCommand[] commands;

        public override IndexingTechnology IndexingTechnology
        {
            get
            {
                return IndexingTechnology.BTree;
            }
        }

        public AccessDatabase()
        {
            SyncRoot = new object();

            Name = "Access 2013";
            CollectionName = "table1";
            Category = "SQL";
            Description = "Access 2013";
            Website = "http://www.microsoft.com/en-us/download/details.aspx?id=13255";
            Color = Color.FromArgb(218, 66, 127);

            Requirements = new string[]
            { 
                "Microsoft Access Database Engine 2010 Redistributable",
                "Microsoft.Office.Interop.Access.Dao.dll" 
            };

            DataDirectory = Path.Combine(MainForm.DATABASES_DIRECTORY, Name);

            OleDbConnectionStringBuilder cb = new OleDbConnectionStringBuilder();
            cb.Provider = "Microsoft.ACE.OLEDB.12.0";
            cb.DataSource = String.Format(@"{0}\{1}.accdb", DataDirectory, Name);
            ConnectionString = cb.ConnectionString;

            Logger = LogManager.GetLogger(Properties.Settings.Default.TestLogger);
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            DBEngine dbEng = new DBEngine();
            Microsoft.Office.Interop.Access.Dao.Database db = dbEng.CreateDatabase(String.Format(@"{0}\{1}", DataDirectory, Name), LanguageConstants.dbLangGeneral);
            db.Close();

            connections = new IDbConnection[flowCount];
            commands = new IDbCommand[flowCount];

            for (int i = 0; i < flowCount; i++)
            {
                IDbConnection connection = new OleDbConnection(ConnectionString);
                connection.Open();
                connections[i] = connection;

                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                commands[i] = CreateCommand(connection);
            }
            connections[0].ExecuteNonQuery(CreateTableQuery());
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            lock (SyncRoot)
            {
                var command = commands[flowID];

                foreach (var kv in flow)
                {
                    var key = Direct(kv.Key);
                    var rec = kv.Value;

                    ((IDbDataParameter)command.Parameters[0]).Value = key;
                    ((IDbDataParameter)command.Parameters[1]).Value = rec.Symbol;
                    ((IDbDataParameter)command.Parameters[2]).Value = rec.Timestamp;
                    ((IDbDataParameter)command.Parameters[3]).Value = rec.Bid;
                    ((IDbDataParameter)command.Parameters[4]).Value = rec.Ask;
                    ((IDbDataParameter)command.Parameters[5]).Value = rec.BidSize;
                    ((IDbDataParameter)command.Parameters[6]).Value = rec.AskSize;
                    ((IDbDataParameter)command.Parameters[7]).Value = rec.Provider;

                    try
                    {
                        command.CommandText = String.Format(@"SELECT * FROM {0} WHERE ID = @ID", CollectionName);
                        var reader = command.ExecuteReader();

                        if (reader.Read())
                            command.CommandText = String.Format(@"UPDATE {0} SET Symbol = @symbol, [TickTimestamp] = @time, Bid = @bid, Ask = @ask, BidSize = @bidSize, AskSize = @askSize, Provider = @provider WHERE ID = @ID", CollectionName);
                        else
                            command.CommandText = String.Format("INSERT INTO {0} VALUES(@ID, '{1}', '{2}', {3}, {4}, {5}, {6}, '{7}');", CollectionName,
                                                                rec.Symbol, rec.Timestamp, rec.Bid, rec.Ask, rec.BidSize, rec.AskSize, rec.Provider);

                        reader.Close();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exc) 
                    {
                        Logger.Error("AccessDatabase insert error ...", exc); 
                    }
                }
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            var command = commands[0];
            command.CommandText = String.Format("SELECT * FROM {0} ORDER BY {1} ASC", CollectionName, "ID");

            IDataReader reader = command.ExecuteReader();

            foreach (var row in reader.Forward())
            {
                byte[] buffer = new byte[64];
                row.GetBytes(0, 0, buffer, 0, 64);

                long key = Reverse(buffer);

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
            reader.Close();
        }

        public override void Finish()
        {
            for (int i = 0; i < connections.Length; i++)
                connections[i].Close();
        }

        [XmlIgnore]
        public override Dictionary<string, string> Settings
        {
            get
            {
                return null;
            }
        }

        private byte[] Direct(Int64 key)
        {
            ulong val = (UInt64)(key + Int64.MaxValue + 1);
            var index = BitConverter.GetBytes(val);

            byte[] buf = new byte[8];
            buf[0] = index[7];
            buf[1] = index[6];
            buf[2] = index[5];
            buf[3] = index[4];
            buf[4] = index[3];
            buf[5] = index[2];
            buf[6] = index[1];
            buf[7] = index[0];

            return buf;
        }

        private Int64 Reverse(byte[] index)
        {
            byte[] buf = new byte[8];
            buf[0] = index[7];
            buf[1] = index[6];
            buf[2] = index[5];
            buf[3] = index[4];
            buf[4] = index[3];
            buf[5] = index[2];
            buf[6] = index[1];
            buf[7] = index[0];

            UInt64 val = BitConverter.ToUInt64(buf, 0);

            return (Int64)(val - (UInt64)Int64.MaxValue - 1);
        }

        private IDbCommand CreateCommand(IDbConnection connection)
        {
            var command = connection.CreateCommand();
            IDbDataParameter key = command.CreateParameter("@ID", DbType.Binary, 64);
            IDbDataParameter symbol = command.CreateParameter("@symbol", DbType.String, 255);
            IDbDataParameter time = command.CreateParameter("@time", DbType.DateTime, 0);
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

        private string CreateTableQuery()
        {
            return String.Format("CREATE TABLE [{0}] (", CollectionName) +
                     "ID BINARY NOT NULL PRIMARY KEY," +
                     "Symbol TEXT NOT NULL," +
                     "TickTimestamp DATETIME NOT NULL," +
                     "Bid DOUBLE NOT NULL," +
                     "Ask DOUBLE NOT NULL," +
                     "BidSize LONG NOT NULL," +
                     "AskSize LONG NOT NULL," +
                     "Provider TEXT NOT NULL)";
        }

        private DbType GetDbType(Type type)
        {
            var typeMap = new Dictionary<Type, DbType>();

            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;

            if (!typeMap.ContainsKey(type))
                return DbType.String;

            return typeMap[type];
        }
    }
}
