//using DatabaseBenchmark.Core;
//using STS.General.Generators;
//using STSdb.Data;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Xml.Serialization;
//using DatabaseBenchmark.Core.Attributes;

//namespace DatabaseBenchmark.Databases
//{
//    public class STSdbDatabase : Database
//    {
//        private StorageEngine engine;
//        private XTable<long, Tick> table;
        
//        public override string IndexingTechnology
//        {
//            get { return "RadixTree"; }
//        }

//        public STSdbDatabase()
//        {
//            SyncRoot = new object();

//            Name = "STSdb";
//            CollectionName = "table1";
//            Category = "NoSQL\\Key-Value Store";
//            Description = "STSdb 3.5.13";
//            Website = "http://www.stsdb.com/";
//            Color = Color.DeepSkyBlue;

//            Requirements = new string[]
//            {
//                "STSdb.dll"
//            };
//        }

//        public override void Open(int flowCount, long flowRecordCount)
//        {
//            engine = StorageEngine.FromFile(Path.Combine(DataDirectory, "test.stsdb"));
//            table = engine.Scheme.CreateOrOpenXTable<long, Tick>(new Locator(CollectionName));

//            engine.Scheme.Commit();
//        }

//        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
//        {
//            lock (SyncRoot)
//            {
//                foreach (var kv in flow)
//                    table[kv.Key] = kv.Value;

//                table.Commit();
//            }
//        }

//        public override IEnumerable<KeyValuePair<long, Tick>> Read()
//        {
//            foreach (var row in table.Forward())
//                yield return new KeyValuePair<long, Tick>(row.Key, row.Record);
//        }

//        public override void Close()
//        {
//            engine.Dispose();
//        }
//    }
//}
