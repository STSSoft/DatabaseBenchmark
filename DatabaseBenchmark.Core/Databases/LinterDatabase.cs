using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.LinterClient;
using DatabaseBenchmark.Core;
using STS.General.Generators;
using STS.General.SQL.Extensions;

namespace DatabaseBenchmark.Databases
{
    public class LinterDatabase : Database
    {
        private LinterDbConnection connection;
        private LinterTable table;

        public override string IndexingTechnology
        {
            get { return "BTree"; }
        }

        public LinterDatabase()
        {
            SyncRoot = new object();

            Name = "Linter";
            CollectionName = "table1";
            Category = "SQL";
            Description = "DBMS Linter SQL Server";
            Website = "http://linter.ru/en/";
            Color = System.Drawing.Color.Blue;

            Requirements = new string[]
            { 
                "inter325.dll",
                "dectic32.dll",
                "System.Data.LinterClient.dll"
            };
        }

        public override void Open()
        {
            var builder = new LinterDbConnectionStringBuilder();
            builder.DataSource = "LOCAL";
            builder.UserID = "SYSTEM";
            builder.Password = "MANAGER";
            ConnectionString = builder.ConnectionString;

            connection = new LinterDbConnection(ConnectionString);
            connection.Open();
        }

        public override ITable<long, Tick> OpenOrCreateTable(string name)
        {
            table = new LinterTable(name, this, connection);
            table.CreateTable();
            table.CreateWriteCommand();
            return table;
        }

        public override void DeleteTable(string name)
        {
            table.DropTable();
        }

        public override void Close()
        {
            connection.Close();
        }
    }

    public class LinterTable : ITable<long, Tick>
    {
        private readonly object SyncRoot = new object();

        private string name;
        private IDatabase database;
        private LinterDbConnection connection;
        private LinterDbCommand writeCommand;

        public string Name
        {
            get { return name; }
        }

        public IDatabase Database
        {
            get { return database; }
        }

        public LinterTable(string name, IDatabase database, LinterDbConnection connection)
        {
            this.name = name;
            this.database = database;
            this.connection = connection;
        }

        public void Write(IEnumerable<KeyValuePair<long, Tick>> records)
        {
            lock (SyncRoot)
            {
                foreach (KeyValuePair<long, Tick> kv in records)
                {
                    long key = kv.Key;
                    Tick tick = kv.Value;

                    writeCommand.Parameters[0].Value = key;
                    writeCommand.Parameters[1].Value = tick.Symbol;
                    writeCommand.Parameters[2].Value = tick.Timestamp;
                    writeCommand.Parameters[3].Value = tick.Bid;
                    writeCommand.Parameters[4].Value = tick.Ask;
                    writeCommand.Parameters[5].Value = tick.BidSize;
                    writeCommand.Parameters[6].Value = tick.AskSize;
                    writeCommand.Parameters[7].Value = tick.Provider;

                    writeCommand.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<KeyValuePair<long, Tick>> Read(long from, long to)
        {
            LinterDbCommand command = connection.CreateCommand();
            command.CommandText = String.Format(
                "SELECT * FROM {0} " +
                "WHERE ID >= {1} AND ID <= {2} " +
                "ORDER BY ID", name, from, to);

            using (LinterDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    long key = reader.GetInt64(0);

                    Tick tick = new Tick();
                    tick.Symbol = reader.GetString(1);
                    tick.Timestamp = reader.GetDateTime(2);
                    tick.Bid = reader.GetDouble(3);
                    tick.Ask = reader.GetDouble(4);
                    tick.BidSize = reader.GetInt32(5);
                    tick.AskSize = reader.GetInt32(6);
                    tick.Provider = reader.GetString(7);

                    yield return new KeyValuePair<long, Tick>(key, tick);
                }
            }
        }

        public IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            LinterDbCommand command = connection.CreateCommand();
            command.CommandText = String.Format(
                "SELECT * FROM {0} " +
                "ORDER BY ID", name);

            using (LinterDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    long key = reader.GetInt64(0);

                    Tick tick = new Tick();
                    tick.Symbol = reader.GetString(1);
                    tick.Timestamp = reader.GetDateTime(2);
                    tick.Bid = reader.GetDouble(3);
                    tick.Ask = reader.GetDouble(4);
                    tick.BidSize = reader.GetInt32(5);
                    tick.AskSize = reader.GetInt32(6);
                    tick.Provider = reader.GetString(7);

                    yield return new KeyValuePair<long, Tick>(key, tick);
                }
            }
        }

        public void Delete(long key)
        {
            LinterDbCommand command = connection.CreateCommand();
            command.CommandText = String.Format(
                "DELETE FROM {0} " +
                "WHERE ID = {1}", name, key);
            command.ExecuteNonQuery();
        }

        public void Delete(long from, long to)
        {
            LinterDbCommand command = connection.CreateCommand();
            command.CommandText = String.Format(
                "DELETE FROM {0} " +
                "WHERE ID >= {1} AND ID <= {2}", name, from, to);
            command.ExecuteNonQuery();
        }

        public Tick this[long key]
        {
            get
            {
                LinterDbCommand command = connection.CreateCommand();
                command.CommandText = String.Format(
                    "SELECT * FROM {0} " +
                    "WHERE ID = {1}", name, key);

                using (LinterDbDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Tick tick = new Tick();
                        tick.Symbol = reader.GetString(1);
                        tick.Timestamp = reader.GetDateTime(2);
                        tick.Bid = reader.GetDouble(3);
                        tick.Ask = reader.GetDouble(4);
                        tick.BidSize = reader.GetInt32(5);
                        tick.AskSize = reader.GetInt32(6);
                        tick.Provider = reader.GetString(7);
                        return tick;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    writeCommand.Parameters[0].Value = key;
                    writeCommand.Parameters[1].Value = value.Symbol;
                    writeCommand.Parameters[2].Value = value.Timestamp;
                    writeCommand.Parameters[3].Value = value.Bid;
                    writeCommand.Parameters[4].Value = value.Ask;
                    writeCommand.Parameters[5].Value = value.BidSize;
                    writeCommand.Parameters[6].Value = value.AskSize;
                    writeCommand.Parameters[7].Value = value.Provider;

                    writeCommand.ExecuteNonQuery();
                }
            }
        }

        public void Close()
        {
        }

        public IEnumerator<KeyValuePair<long, Tick>> GetEnumerator()
        {
            return Read().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int CreateTable()
        {
            LinterDbCommand command = connection.CreateCommand();
            command.CommandText = String.Format(
                "CREATE OR REPLACE TABLE {0} (", name) +
                "ID bigint primary key," +
                "Symbol varchar(255)," +
                "Time date," +
                "Bid double," +
                "Ask double," +
                "BidSize int," +
                "AskSize int," +
                "Provider varchar(255))";
            return command.ExecuteNonQuery();
        }

        public void CreateWriteCommand()
        {
            LinterDbCommand command = connection.CreateCommand();
            command.Parameters.Add(":ID", ELinterDbType.Bigint);
            command.Parameters.Add(":symbol", ELinterDbType.NChar, 510);
            command.Parameters.Add(":time", ELinterDbType.Date);
            command.Parameters.Add(":bid", ELinterDbType.Double);
            command.Parameters.Add(":ask", ELinterDbType.Double);
            command.Parameters.Add(":bidSize", ELinterDbType.Int);
            command.Parameters.Add(":askSize", ELinterDbType.Int);
            command.Parameters.Add(":provider", ELinterDbType.NChar, 510);

            command.CommandType = CommandType.Text;
            command.CommandText = String.Format(@"
                Merge into {0}
                Using (select :ID (bigint) as id1, :symbol (varchar(255)), :time (date), :bid (double), :ask (double), :bidSize (int), :askSize (int), :provider (varchar(255))) as src
                On {0}.id=src.id1
                WHEN MATCHED THEN
                 UPDATE SET {0}.id=:ID, {0}.Symbol=:symbol, {0}.Time=:time, {0}.Bid=:bid, {0}.Ask=:ask, {0}.BidSize=:bidSize, {0}.AskSize=:askSize, {0}.Provider=:provider
                WHEN NOT MATCHED THEN
                 INSERT ({0}.id, {0}.Symbol, {0}.Time, {0}.Bid, {0}.Ask, {0}.BidSize, {0}.AskSize, {0}.Provider) VALUES (:ID, :symbol, :time, :bid, :ask, :bidSize, :askSize, :provider)",
                name);

            command.Prepare();

            writeCommand = command;
        }

        public int DropTable()
        {
            LinterDbCommand command = connection.CreateCommand();
            command.CommandText = String.Format(
                "DROP TABLE {0} ", name);
            return command.ExecuteNonQuery();
        }
    }
}