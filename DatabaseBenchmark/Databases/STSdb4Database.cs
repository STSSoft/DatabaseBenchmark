using STS.General.Generators;
using STSdb4.Database;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DatabaseBenchmark.Databases
{
    public class STSdb4Database : Database
    {
        private IStorageEngine engine;
        private ITable<long, Tick> table;

        public int CacheSize { get; set; }
        public bool InMemoryDatabase { get; set; }

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

        public override void Init(int flowCount, long flowRecordCount)
        {
            engine = InMemoryDatabase ? STSdb4.Database.STSdb.FromMemory() : STSdb4.Database.STSdb.FromFile(Path.Combine(DataDirectory, "test.stsdb4"));
            ((StorageEngine)engine).CacheSize = CacheSize;

            table = engine.OpenXTable<long, Tick>(CollectionName);
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            lock (SyncRoot)
            {
                foreach (var kv in flow)
                    table[kv.Key] = kv.Value;

                engine.Commit();
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            return engine.OpenXTable<long, Tick>(CollectionName).Forward();
        }

        public override void Finish()
        {
            engine.Close();
        }
    }
}
