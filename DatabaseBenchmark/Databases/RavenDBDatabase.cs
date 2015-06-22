using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Database.Extensions;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Databases
{
    public class RavenDBDatabase : Database
    {
        private int recordsCount;
        private EmbeddableDocumentStore dataStore;
        private BulkInsertOperation[] bulkInsertSessions;

        public override IndexingTechnology IndexingTechnology
        {
            get { return IndexingTechnology.BPlusTree; }
        }

        public RavenDBDatabase()
        {
            Name = "RavenDB";
            CollectionName = "database";
            Category = "NoSQL\\Document Store";
            Description = "3.0";
            Website = "http://ravendb.net/";
            Color = Color.FromArgb(168, 25, 32);

            Requirements = new string[] 
            { 
                "Raven.Abstractions.dll",
                "Raven.Client.dll",
                "Raven.Database.dll"
            };
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            recordsCount = 0;
            bulkInsertSessions = new BulkInsertOperation[flowCount];

            dataStore = new EmbeddableDocumentStore { DataDirectory = this.DataDirectory };
            dataStore.Initialize();
            IndexCreation.CreateIndexes(typeof(TickRecordIndex).Assembly, dataStore);

            for (int i = 0; i < flowCount; i++)
            {
                var session = dataStore.BulkInsert();
                bulkInsertSessions[i] = session;
            }
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            using (bulkInsertSessions[flowID])
            {
                foreach (var kv in flow)
                {
                    TickRecord entity = new TickRecord(kv.Key, kv.Value);
                    bulkInsertSessions[flowID].Store(entity);

                    recordsCount++;
                }
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            int skip = 0;
            int currentRecords = 0;
            int takeCount = 1000;

            var session = dataStore.OpenSession();
            session.Advanced.MaxNumberOfRequestsPerSession = Int32.MaxValue;

            while (currentRecords < recordsCount)
            {
                var records = session.Query<TickRecord, TickRecordIndex>().Customize(x => x.WaitForNonStaleResultsAsOfNow()).Skip(skip).Take(takeCount).OrderBy(x => x.Key);

                foreach (var kv in records)
                {
                    yield return new KeyValuePair<long, Tick>(kv.Key, kv.Record);
                    currentRecords++;
                }

                skip += takeCount;
            }
            session.Dispose();
        }

        public override void Finish()
        {
            foreach (var item in bulkInsertSessions)
                item.Dispose();

            dataStore.Dispose();

            // Special workaround, because RavenDB denies acces to its data directory.
            foreach (var file in Directory.GetFiles(DataDirectory))
                File.Delete(file);

            IOExtensions.DeleteDirectory(DataDirectory);
        }

        public override long Size
        {
            get
            {
                string directoryPath = Path.Combine(DataDirectory, "Data");
                FileInfo info = new FileInfo(directoryPath);

                return info.Length;
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

        public class TickRecord
        {
            public long Key { get; set; }
            public Tick Record { get; set; }

            public TickRecord(long key, Tick record)
            {
                Key = key;
                Record = record;
            }
        }

        public class TickRecordIndex : AbstractIndexCreationTask<TickRecord>
        {
            public TickRecordIndex()
            {
                Map = records => from record in records
                                 select new
                                 {
                                     record.Key
                                 };

                Index(x => x.Key, FieldIndexing.Default);
                Sort(x => x.Key, SortOptions.Long);
            }
        }
    }
}
