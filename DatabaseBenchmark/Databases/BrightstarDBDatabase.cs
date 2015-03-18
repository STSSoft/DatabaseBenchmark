using BrightstarDB.Client;
using BrightstarDB.EntityFramework;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DatabaseBenchmark.Databases
{
    //Manual commit, very slow and hard for work.
    public class BrightstarDBDatabase : Database
    {
        private MyEntityContext[] contexts;

        /// <summary>
        /// Specifies how many records are inserted with every batch.
        /// </summary>
        public int InsertsPerQuery { get; set; }

        public BrightstarDBDatabase()
        {
            DatabaseName = "BrightstarDB";
            DatabaseCollection = "table1";
            Category = "NoSQL\\Graph Databases";
            Description = "BrightstarDB 1.9.1 Stable - Mon Feb 2, 2015 at 10:00 AM";
            Website = "http://www.brightstardb.com/";
            Color = Color.LightGoldenrodYellow;

            Requirements = new string[]
            { 
                "BrightstarDB.dll"
            };

            InsertsPerQuery = 10000;
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            contexts = new MyEntityContext[flowCount];

            string connectionString = string.Format(@"Type=embedded;storesDirectory={0};StoreName={1}", DataDirectory, DatabaseCollection);

            for (int i = 0; i < flowCount; i++)
                contexts[i] = new MyEntityContext(connectionString, true);
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            int count = 0;

            foreach (var item in flow)
            {
                var element = contexts[flowID].Ticks.Create();
                element.Key = item.Key;
                element.Symbol = item.Value.Symbol;
                element.Timestamp = item.Value.Timestamp;
                element.Bid = item.Value.Bid;
                element.Ask = item.Value.Ask;
                element.BidSize = item.Value.BidSize;
                element.AskSize = item.Value.AskSize;
                element.Provider = item.Value.Provider;

                count++;

                if (count == InsertsPerQuery)
                {
                    try
                    {
                        contexts[flowID].Save();
                    }
                    catch (TransactionPreconditionsFailedException)
                    {
                        contexts[flowID].Refresh(RefreshMode.StoreWins, contexts[flowID].Ticks);
                    }

                    count = 0;
                }
            }

            contexts[flowID].Save();
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            foreach (var flow in contexts[0].Ticks.OrderBy(tick => tick.Key))
                yield return new KeyValuePair<long, Tick>(flow.Key, new Tick(flow.Symbol, flow.Timestamp, flow.Bid, flow.Ask, flow.BidSize, flow.AskSize, flow.Provider));
        }

        public override void Finish()
        {
            foreach (var context in contexts)
                context.Dispose();
        }

        public class MyEntityContext : BrightstarEntityContext
        {
            static MyEntityContext()
            {
                ReflectionMappingProvider provider = new ReflectionMappingProvider();
                provider.AddMappingsForType(EntityMappingStore.Instance, typeof(ITick));
                EntityMappingStore.Instance.SetImplMapping<ITick, TickRecord>();
            }

            public MyEntityContext(string connectionString, bool? enableOptimisticLocking = null, string updateGraphUri = null, IEnumerable<string> datasetGraphUris = null, string versionGraphUri = null)
                : base(connectionString, enableOptimisticLocking, updateGraphUri, datasetGraphUris, versionGraphUri)
            {
                Ticks = new BrightstarEntitySet<DatabaseBenchmark.Databases.ITick>(this);
            }

            public void Save()
            {
                base.SaveChanges();
                base.Cleanup();
            }

            public IEntitySet<DatabaseBenchmark.Databases.ITick> Ticks
            {
                get;
                private set;
            }

            public IEntitySet<T> EntitySet<T>() where T : class
            {
                var itemType = typeof(T);
                if (typeof(T).Equals(typeof(DatabaseBenchmark.Databases.ITick)))
                {
                    return (IEntitySet<T>)this.Ticks;
                }

                throw new InvalidOperationException(typeof(T).FullName + " is not a recognized entity interface type.");
            }
        }

        public class TickRecord : BrightstarEntityObject, ITick
        {
            public TickRecord(BrightstarEntityContext context, BrightstarDB.Client.IDataObject dataObject) :
                base(context, dataObject)
            {
            }

            public TickRecord(BrightstarEntityContext context) :
                base(context, typeof(Tick))
            {
            }

            public TickRecord()
                : base()
            {
            }

            #region Implementation of DatabaseBenchmark.Databases.ITick

            public System.Int64 Key
            {
                get { return GetRelatedProperty<System.Int64>("Key"); }
                set { SetRelatedProperty("Key", value); }
            }

            public System.String Symbol
            {
                get { return GetRelatedProperty<System.String>("Symbol"); }
                set { SetRelatedProperty("Symbol", value); }
            }

            public System.DateTime Timestamp
            {
                get { return GetRelatedProperty<System.DateTime>("Timestamp"); }
                set { SetRelatedProperty("Timestamp", value); }
            }

            public System.Double Bid
            {
                get { return GetRelatedProperty<System.Double>("Bid"); }
                set { SetRelatedProperty("Bid", value); }
            }

            public System.Double Ask
            {
                get { return GetRelatedProperty<System.Double>("Ask"); }
                set { SetRelatedProperty("Ask", value); }
            }

            public System.Int32 BidSize
            {
                get { return GetRelatedProperty<System.Int32>("BidSize"); }
                set { SetRelatedProperty("BidSize", value); }
            }

            public System.Int32 AskSize
            {
                get { return GetRelatedProperty<System.Int32>("AskSize"); }
                set { SetRelatedProperty("AskSize", value); }
            }

            public System.String Provider
            {
                get { return GetRelatedProperty<System.String>("Provider"); }
                set { SetRelatedProperty("Provider", value); }
            }
            #endregion
        }
    }

    [Entity]
    public interface ITick
    {
        long Key { get; set; }
        string Symbol { get; set; }
        DateTime Timestamp { get; set; }
        double Bid { get; set; }
        double Ask { get; set; }
        int BidSize { get; set; }
        int AskSize { get; set; }
        string Provider { get; set; }
    }
}
