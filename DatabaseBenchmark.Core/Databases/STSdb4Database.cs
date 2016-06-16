using DatabaseBenchmark.Core;
using STS.General.Generators;
using STSdb4.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DatabaseBenchmark.Core.Attributes;
using System.Collections;
using System;

namespace DatabaseBenchmark.Databases
{
    public class STSdb4Database : Database
    {
        private IStorageEngine engine;
        private DatabaseBenchmark.Core.ITable<long, Tick> table;

        [Category("Settings")]
        [DbParameter]
        public int CacheSize { get; set; }

        [Category("Settings")]
        public bool InMemoryDatabase { get; set; }

        public override string IndexingTechnology
        {
            get { return "WaterfallTree"; }
        }

        public STSdb4Database()
        {
            SyncRoot = new object();

            Name = "STSdb 4.0";
            CollectionName = "table1";
            Category = @"NoSQL\Key-Value Store";
            Description = "STSdb 4.0";
            Website = "http://www.stsdb.com/";
            Color = Color.CornflowerBlue;

            Requirements = new string[] 
            {
                "STSdb4.dll"
            };

            CacheSize = 64;
        }

        public override void Open()
        {
            engine = InMemoryDatabase ? STSdb4.Database.STSdb.FromMemory() : STSdb4.Database.STSdb.FromFile(Path.Combine(DataDirectory, "test.stsdb4"));
            ((StorageEngine)engine).CacheSize = CacheSize;

            table = new Table(CollectionName, this, engine);
        }

        public override Core.ITable<long, Tick> OpenOrCreateTable(string name)
        {
            return new Table(name, this, engine);
        }

        public override void DeleteTable(string name)
        {
            engine.Delete(name);
        }

        public override void Close()
        {
            engine.Close();
        }
    }

    public class Table : DatabaseBenchmark.Core.ITable<long, Tick>
    {
        private readonly object SyncRoot = new object();

        private string name;
        private IDatabase database;

        private IStorageEngine engine;
        private STSdb4.Database.ITable<long, Tick> table;

        public string Name
        {
            get { return name; }
        }

        public IDatabase Database
        {
            get { return database; }
        }

        public Table(string name, IDatabase database, IStorageEngine engine)
        {
            this.name = name;
            this.database = database;
            this.engine = engine;

            table = engine.OpenXTable<long, Tick>(name);
        }

        public void Write(IEnumerable<KeyValuePair<long, Tick>> records)
        {
            lock (SyncRoot)
            {
                foreach (var record in records)
                    table[record.Key] = record.Value;

                engine.Commit();
            }
        }

        public IEnumerable<KeyValuePair<long, Tick>> Read(long from, long to)
        {
            return table.Forward(from, true, to, true);
        }

        public IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            return table.Forward();
        }

        public void Delete(long key)
        {
            table.Delete(key);
        }

        public void Delete(long from ,long to)
        {
            table.Delete(from, to);
        }

        public Tick this[long key]
        {
            get
            {
                return table[key];
            }
            set
            {
                table[key] = value;
            }
        }

        public void Close()
        {
        }

        public IEnumerator<KeyValuePair<long, Tick>> GetEnumerator()
        {
            return table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
