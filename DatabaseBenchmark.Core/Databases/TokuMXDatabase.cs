//using MongoDB.Bson;
//using MongoDB.Driver;
//using MongoDB.Driver.Builders;
//using STS.General.Extensions;
//using STS.General.Generators;
//using System;
//using DatabaseBenchmark.Core;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Linq;
//using System.Xml.Serialization;
//using DatabaseBenchmark.Core.Attributes;

//namespace DatabaseBenchmark.Databases
//{
//    public class TokuMXDatabase : Database
//    {
//        private MongoClient[] clients;
//        private IMongoCollection<Row>[] collections;

//        [DbParameter]
//        public int InsertsPerQuery { get; set; }
//        [DbParameter]
//        public int WriteConcern { get; set; }
//        [DbParameter]
//        public bool PerformReplace { get; set; }

//        public override string IndexingTechnology
//        {
//            get { return "FractalTree"; }
//        }

//        public TokuMXDatabase()
//        {
//            Name = "TokuMX";
//            CollectionName = "collection";
//            Category = "NoSQL\\Document Store";
//            Description = "TokuMX v2.0";
//            Website = "http://www.tokutek.com/tokumx-for-mongodb/";
//            Color = Color.Chartreuse;

//            Requirements = new string[] 
//            { 
//                "MongoDB.Bson.dll", 
//                "MongoDB.Driver.dll",
//                "MongoDB.Driver.Core.dll",
//                "MongoDB.Driver.Legacy.dll"
//            };

//            WriteConcern = 1;
//            InsertsPerQuery = 5000;
//            PerformReplace = true;

//            MongoUrlBuilder builder = new MongoUrlBuilder();
//            builder.MaxConnectionIdleTime = TimeSpan.FromHours(1);
//            builder.MaxConnectionLifeTime = TimeSpan.FromHours(1);
//            builder.Server = new MongoServerAddress("localhost");
//            builder.W = WriteConcern;

//            ConnectionString = builder.ToString();
//        }

//        public override void Open(int flowCount, long flowRecordCount)
//        {
//            clients = new MongoClient[flowCount];
//            collections = new IMongoCollection<Row>[flowCount];

//            for (int i = 0; i < flowCount; i++)
//            {
//                MongoClient client = new MongoClient(ConnectionString);
//                IMongoDatabase database = client.GetDatabase("test");

//                database.DropCollectionAsync(CollectionName).GetAwaiter().GetResult();

//                IMongoCollection<Row> collection = database.GetCollection<Row>(CollectionName);

//                collection.Indexes.CreateOneAsync(new IndexKeysDefinitionBuilder<Row>().Ascending("_id"));
//                collection.WithWriteConcern(new WriteConcern(WriteConcern));

//                collections[i] = collection;
//                clients[i] = client;
//            }
//        }

//        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
//        {
//            List<WriteModel<Row>> buffer = new List<WriteModel<Row>>(InsertsPerQuery);

//            foreach (var kv in flow)
//            {
//                Row row = new Row(kv.Key, kv.Value);

//                if (PerformReplace)
//                {
//                    ReplaceOneModel<Row> model = new ReplaceOneModel<Row>(new ObjectFilterDefinition<Row>(row), row);
//                    model.IsUpsert = true;

//                    buffer.Add(model);
//                }
//                else
//                {
//                    InsertOneModel<Row> model2 = new InsertOneModel<Row>(row);
//                    buffer.Add(model2);
//                }

//                if (buffer.Count == InsertsPerQuery)
//                {
//                    var result = collections[flowID].BulkWriteAsync(buffer).Result;
//                    buffer.Clear();
//                }
//            }

//            if (buffer.Count != 0)
//                collections[flowID].BulkWriteAsync(buffer);
//        }

//        public override IEnumerable<KeyValuePair<long, Tick>> Read()
//        {
//            IAsyncCursor<Row> enumerator = collections[0].Aggregate().Sort(new SortDefinitionBuilder<Row>().Ascending("_id")).ToCursorAsync().Result;

//            while (enumerator.MoveNextAsync().Result)
//            {
//                foreach (var item in enumerator.Current)
//                    yield return new KeyValuePair<long, Tick>(item._id, item.Record);
//            }
//        }

//        public override void Close()
//        {
//        }

//        public override long Size
//        {
//            get
//            {
//                long sum = 0;

//                // TODO: Finding another way to take the size.
//                var stat = clients[0].GetServer().GetDatabase("test").GetCollection<Row>(CollectionName).GetStats();
//                sum += stat.StorageSize + stat.TotalIndexSize;

//                return sum;
//            }
//        }

//        private class Row
//        {
//            public long _id { get; set; }
//            public Tick Record { get; set; }

//            public Row()
//            {
//            }

//            public Row(long key, Tick record)
//            {
//                _id = key;
//                Record = record;
//            }
//        }
//    }
//}
