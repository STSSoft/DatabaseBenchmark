using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Views;
using STS.General.Generators;
using System;
using System.Collections.Generic;

namespace DatabaseBenchmark.Databases
{
    public class CouchbaseDatabase : Database
    {
        private IBucket[] buckets;
        private Cluster cluster;

        public string DocDesignName { get; set; }
        public string ViewName { get; set; }

        public CouchbaseDatabase()
        {
            SyncRoot = new object();

            Name = "Couchbase";
            CollectionName = "default";
            Category = "NoSQL\\Key-Value Store";
            Description = "Couchbase 3.0.2 + Couchbase-Net-Client-2.0.2";
            Website = "http://www.couchbase.com/";
            Color = System.Drawing.Color.OrangeRed;

            Requirements = new string[]
            {
                "Newtonsoft.Json.dll",
                "Common.Logging.dll",
                "Couchbase.NetClient.dll",

                @"Create development view with design document name = DocDesignName (example: _design/dev_Test where the document name is Test) 
                and view name = ViewName (example: Test)"
            };

            ConnectionString = "http://localhost:8091/";

            DocDesignName = "Test";
            ViewName = "Test";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            buckets = new IBucket[flowCount];

            ClientConfiguration client = new ClientConfiguration();
            client.Servers.Add(new Uri(ConnectionString));

            cluster = new Cluster(client);

            for (int i = 0; i < flowCount; i++)
                buckets[i] = cluster.OpenBucket(CollectionName);

            IBucket bucket = buckets[0];
            IViewQuery query = bucket.CreateQuery(DocDesignName, ViewName, false);

            // TODO: find a faster way for dropping the database.
            foreach (var item in buckets[0].Query<dynamic>(query).Rows)
                buckets[0].Remove(item.Key.ToString());
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            lock (SyncRoot)
            {
                foreach (var kv in flow)
                    buckets[flowID].Upsert(kv.Key.ToString(), kv.Value);
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            IBucket bucket = buckets[0];
            IViewQuery query = bucket.CreateQuery(DocDesignName, ViewName, false).Asc();

            foreach (var item in buckets[0].Query<dynamic>(query).Rows)
            {
                long key = Int64.Parse(item.Key.ToString());
                Tick record = item.Value;

                yield return new KeyValuePair<long, Tick>(key, record);
            }
        }

        public override long Size
        {
            get { return 0; }
        }

        public override void Finish()
        {
            foreach (var item in buckets)
                cluster.CloseBucket(item);
        }
    }
}
