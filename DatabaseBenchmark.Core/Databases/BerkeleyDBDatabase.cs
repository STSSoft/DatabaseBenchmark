//using BerkeleyDB;
//using STS.General.Generators;
//using System;
//using DatabaseBenchmark.Core;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Xml.Serialization;
//using DatabaseBenchmark.Core.Attributes;

//namespace DatabaseBenchmark.Databases
//{
//    public class BerkeleyDBDatabase : Core.Database
//    {
//        private BTreeDatabase database;

//        [DbParameter]
//        public int CacheChunks { get; set; }
//        [DbParameter]
//        public uint CacheSizeGigabytes { get; set; }
//        [DbParameter]
//        public uint PageSizeKilobytes { get; set; }

//        public override string IndexingTechnology
//        {
//            get { return "BTree"; }
//        }

//        public BerkeleyDBDatabase()
//        {
//            SyncRoot = new object();

//            Name = "Berkeley DB";
//            CollectionName = "table";
//            Category = "NoSQL\\Key-Value Store";
//            Description = "Berkeley DB 12c Release 1 Library Version 12.1.6.0";
//            Website = "http://www.oracle.com/technology/products/berkeley-db";
//            Color = Color.FromArgb(249, 0, 0);

//            Requirements = new string[] 
//            { 
//                "libdb60.dll",
//                "libdb_csharp60.dll",
//                "libdb_dotnet60.dll"
//            };

//            CacheSizeGigabytes = 30;
//            CacheChunks = 3;
//            PageSizeKilobytes = 64;
//        }

//        public override void Open(int flowCount, long flowRecordCount)
//        {
//            BTreeDatabaseConfig config = new BTreeDatabaseConfig();

//            config.Creation = CreatePolicy.IF_NEEDED;
//            config.CacheSize = new CacheInfo(CacheSizeGigabytes, 0, CacheChunks);
//            config.PageSize = PageSizeKilobytes * 1024;
//            config.BTreeCompare = new EntryComparisonDelegate(CompareFunctionKeyByteArray);
//            config.Duplicates = DuplicatesPolicy.NONE;

//            string fileName = Path.Combine(DataDirectory, string.Format("{0}.oracle", CollectionName));
//            database = BTreeDatabase.Open(fileName, config);
//        }

//        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
//        {
//            lock (SyncRoot) 
//            {
//                foreach (var item in flow)
//                {
//                    DatabaseEntry key = new DatabaseEntry(BitConverter.GetBytes(item.Key));
//                    DatabaseEntry value = new DatabaseEntry(FromTick(item.Value));

//                    database.Put(key, value);
//                }

//                database.Sync();
//            }
//        }

//        public override IEnumerable<KeyValuePair<long, Tick>> Read()
//        {
//            Cursor cursor = database.Cursor();

//            foreach (var kv in cursor)
//            {
//                long key = BitConverter.ToInt64(kv.Key.Data, 0);
//                Tick record = ToTick(kv.Value.Data);

//                yield return new KeyValuePair<long, Tick>(key, record);
//            }

//            cursor.Close();
//        }

//        public override void Close()
//        {
//            database.Close();
//        }

//        #region Helper Methods

//        private int CompareFunctionKeyByteArray(DatabaseEntry dbt1, DatabaseEntry dbt2)
//        {
//            byte[] key1 = dbt1.Data;
//            byte[] key2 = dbt2.Data;

//            long key3 = BitConverter.ToInt64(key1, 0);
//            long key4 = BitConverter.ToInt64(key2, 0);

//            return Comparer<long>.Default.Compare(key3, key4);
//        }

//        private byte[] FromTick(Tick tick)
//        {
//            using (var ms = new MemoryStream())
//            {
//                BinaryWriter writer = new BinaryWriter(ms);

//                writer.Write(tick.Symbol);
//                writer.Write(tick.Timestamp.Ticks);
//                writer.Write(tick.Bid);
//                writer.Write(tick.Ask);
//                writer.Write(tick.BidSize);
//                writer.Write(tick.AskSize);
//                writer.Write(tick.Provider);

//                return ms.ToArray();
//            }
//        }

//        private Tick ToTick(byte[] value)
//        {
//            Tick tick = new Tick();

//            using (MemoryStream stream = new MemoryStream(value))
//            {
//                BinaryReader reader = new BinaryReader(stream);

//                tick.Symbol = reader.ReadString();
//                tick.Timestamp = new DateTime(reader.ReadInt64());
//                tick.Bid = reader.ReadDouble();
//                tick.Ask = reader.ReadDouble();
//                tick.BidSize = reader.ReadInt32();
//                tick.AskSize = reader.ReadInt32();
//                tick.Provider = reader.ReadString();
//            }

//            return tick;
//        }

//        #endregion
//    }
//}